using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rinsen.IdentityProvider.Outback.Entities;

namespace Rinsen.IdentityProvider.Outback;

public class ScopeService
{
    private readonly OutbackDbContext _outbackDbContext;

    public ScopeService(OutbackDbContext outbackDbContext)
    {
        _outbackDbContext = outbackDbContext;
    }

    public Task<List<OutbackScope>> GetAllAsync()
    {
        return _outbackDbContext.Scopes
            .Include(m => m.ScopeClaims).ToListAsync();
    }

    public Task<OutbackScope?> GetScopeAsync(int id)
    {
        return _outbackDbContext.Scopes
            .Include(m => m.ScopeClaims).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<int> CreateScopeAsync(string displayName, string scopeName, string audience, string description, bool showInDiscoveryDocument, bool claimsInIdToken, bool enabled)
    {
        var scope = await _outbackDbContext.Scopes.SingleOrDefaultAsync(m => m.ScopeName == scopeName);

        if (scope != default)
        {
            scope.DisplayName = displayName;
            scope.Audience = audience;
            scope.Description = description;
            scope.ShowInDiscoveryDocument = showInDiscoveryDocument;
            scope.ClaimsInIdToken = claimsInIdToken;
            scope.Enabled = enabled;

            await _outbackDbContext.SaveChangesAsync();

            return scope.Id;
        }

        scope = new OutbackScope
        {
            DisplayName = displayName,
            ScopeName = scopeName,
            Description = description,
            ShowInDiscoveryDocument = showInDiscoveryDocument,
            Audience = audience,
            ClaimsInIdToken = claimsInIdToken,
            Enabled = enabled
        };

        await _outbackDbContext.AddAsync(scope);

        await _outbackDbContext.SaveChangesAsync();

        return scope.Id;
    }

    public async Task UpdateScopeAsync(int id, OutbackScope updatedScope)
    {
        var scope = await GetScopeAsync(id);

        if (scope == default)
        {
            throw new Exception($"Client with id {id} not found");
        }

        if (ScopeParametersNotEquals(scope, updatedScope))
        {
            UpdateScopeParameters(scope, updatedScope);
        }

        UpdateScopeClaims(scope, updatedScope);

        await _outbackDbContext.SaveChangesAsync();
    }

    public async Task DeleteScopeAsync(int id)
    {
        var scope = await _outbackDbContext.Scopes.SingleOrDefaultAsync(m => m.Id == id);

        if (scope == default)
        {
            throw new Exception($"Client with id {id} not found");
        }

        _outbackDbContext.Scopes.Remove(scope);

        await _outbackDbContext.SaveChangesAsync();
    }

    private static bool ScopeParametersNotEquals(OutbackScope scope, OutbackScope updatedScope)
    {
        return scope.Audience != updatedScope.Audience
            || scope.Enabled != updatedScope.Enabled
            || scope.DisplayName != updatedScope.DisplayName
            || scope.Description != updatedScope.Description
            || scope.ScopeName != updatedScope.ScopeName
            || scope.ShowInDiscoveryDocument != updatedScope.ShowInDiscoveryDocument;
    }

    private static void UpdateScopeParameters(OutbackScope scope, OutbackScope updatedScope)
    {
        scope.Audience = updatedScope.Audience;
        scope.Enabled = updatedScope.Enabled;
        scope.DisplayName = updatedScope.DisplayName;
        scope.Description = updatedScope.Description;
        scope.ScopeName = updatedScope.ScopeName;
        scope.ShowInDiscoveryDocument = updatedScope.ShowInDiscoveryDocument;
    }

    private void UpdateScopeClaims(OutbackScope scope, OutbackScope updatedScope)
    {
        var existingClaims = new List<OutbackScopeClaim>();

        foreach (var claim in updatedScope.ScopeClaims)
        {
            var existingScope = scope.ScopeClaims.SingleOrDefault(m => m.Id == claim.Id);

            if (existingScope == default)
            {
                var newScopeClaim = new OutbackScopeClaim
                {
                    Type = claim.Type,
                    Description = claim.Description,
                };

                scope.ScopeClaims.Add(newScopeClaim);

                existingClaims.Add(newScopeClaim);
            }
            else
            {
                if (existingScope.Deleted != null)
                {
                    existingScope.Deleted = null;
                }

                if (existingScope.Description != claim.Description ||
                existingScope.Type != claim.Type)
                {
                    existingScope.Description = claim.Description;
                    existingScope.Type = claim.Type;
                }

                existingClaims.Add(existingScope);
            }
        }

        var existingClaimsToDelete = scope.ScopeClaims.Except(existingClaims).ToList();

        if (existingClaimsToDelete.Any())
        {
            _outbackDbContext.ScopeClaims.RemoveRange(existingClaimsToDelete);
        }
    }

}
