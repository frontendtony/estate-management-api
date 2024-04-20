using System.Text;
using System.Text.Json;
using Eros.Application.Features.Invitations.Commands;
using Eros.Domain.Aggregates.Estates;
using Eros.Domain.Aggregates.Invitations;
using Eros.Domain.Aggregates.Roles;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eros.Application.Features.Invitations.CommandHandlers;

public class SendInvitationsCommandHandler(
    IRoleReadRepository _roleReadRepository,
    IEstateUserReadRepository _estateUserReadRepository,
    ILogger<SendInvitationsCommandHandler> _logger
) : IRequestHandler<SendInvitationsCommand, bool>
{
    public async Task<bool> Handle(SendInvitationsCommand request, CancellationToken cancellationToken)
    {
        var estateUser = await _estateUserReadRepository.GetByEstateIdAndUserIdAsync(request.EstateId, request.SenderId, cancellationToken);

        if (estateUser == null)
        {
            _logger.LogError("User is not a member of the estate. {EstateId} {SenderId}", request.EstateId, request.SenderId);
            throw new Exception("User is not a member of the estate.");
        }

        var role = await _roleReadRepository.GetByIdAsync(request.RoleId);

        if (role == null)
        {
            _logger.LogError("The role does not exist. {RoleId}", request.RoleId);
            throw new Exception("The role does not exist.");
        }

        var rolePermisionIds = role.Permissions.Select(p => p.Id);

        var hasPermissions = await _roleReadRepository.HasPermissionsAsync(estateUser.RoleId, rolePermisionIds, cancellationToken);

        if (!hasPermissions)
        {
            // User can only assign roles with permissions that he has
            _logger.LogError("User does not have the required permissions to assign the role. {EstateId} {SenderId} {RoleId}", request.EstateId, request.SenderId, request.RoleId);
            throw new Exception("User does not have the required permissions to assign the role.");
        }

        // Create invitations
        var invitations = request.Emails.Select(email => new Invitation
        {
            Id = Guid.NewGuid(),
            Email = email,
            RoleId = request.RoleId,
            EstateId = request.EstateId,
            CreatedBy = request.SenderId,
            // Generate a base64 hash that includes the estate name, estate id, inviter name, role name, and expiration date as json
            Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                request.EstateId,
                EstateName = estateUser.Estate.Name,
                InviterName = estateUser.User.FirstName,
                RoleName = role.Name,
                ExpirationDate = DateTime.UtcNow.AddDays(7)
            })))
        }).ToArray();

        // Send invitation emails
        await SendInvitationEmails(invitations);
        return true;
    }

    private Task SendInvitationEmails(Invitation[] invitations)
    {
        _logger.LogInformation("Sending invitation emails...");

        foreach (var invitation in invitations)
        {
            _logger.LogInformation("Invitation: {Email} {Code}", invitation.Email, invitation.Code);
        }
        return Task.CompletedTask;
    }
}