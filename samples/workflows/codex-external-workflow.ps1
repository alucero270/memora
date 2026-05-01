param(
    [string]$BaseUrl = "http://127.0.0.1:5081",
    [string]$ProjectId = "demo-project",
    [string]$TaskDescription = "Validate the Codex external workflow against the shared Memora contract.",
    [string]$ProposalArtifactId = ("ADR-" + (Get-Date -Format "yyyyMMddHHmmss")),
    [string]$OutcomeArtifactId = ("OUT-" + (Get-Date -Format "yyyyMMddHHmmss"))
)

function Invoke-MemoraRequest
{
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("Get", "Post")]
        [string]$Method,
        [Parameter(Mandatory = $true)]
        [string]$Uri,
        [object]$Body = $null
    )

    try
    {
        if ($null -eq $Body)
        {
            return Invoke-RestMethod -Method $Method -Uri $Uri
        }

        return Invoke-RestMethod `
            -Method $Method `
            -Uri $Uri `
            -ContentType "application/json" `
            -Body ($Body | ConvertTo-Json -Depth 10)
    }
    catch
    {
        if ($_.Exception.Response)
        {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            $reader.Close()
            throw "Request to '$Uri' failed: $responseBody"
        }

        throw
    }
}

$project = Invoke-MemoraRequest -Method Get -Uri "$BaseUrl/api/projects/$ProjectId"

$contextRequest = @{
    projectId = $ProjectId
    taskDescription = $TaskDescription
    includeDraftArtifacts = $false
    includeLayer3History = $false
    focusArtifactIds = @()
    focusTags = @("runtime")
    maxLayer2Artifacts = 10
    maxLayer3Artifacts = 10
}

$context = Invoke-MemoraRequest -Method Post -Uri "$BaseUrl/api/context" -Body $contextRequest

$artifactIds = @(
    $context.bundle.layers |
        ForEach-Object { $_.artifacts } |
        ForEach-Object { $_.artifact.id }
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

$today = Get-Date -Format "yyyy-MM-dd"
$retrievedArtifacts = if ($artifactIds.Count -gt 0)
{
    $artifactIds -join ", "
}
else
{
    "none"
}

$proposalRequest = @{
    projectId = $ProjectId
    artifactId = $ProposalArtifactId
    artifactType = 2
    content = @{
        title = "Codex external workflow proposal"
        provenance = "codex"
        reason = "Codex consumed deterministic Memora context and submitted a reviewable proposal through the shared contract."
        tags = @("codex", "workflow", "runtime")
        sections = @{
            Context = "Codex requested deterministic context for '$TaskDescription' and retrieved: $retrievedArtifacts."
            Decision = "Use the current shared Memora contract through the local companion API for the first operational Codex workflow."
            "Alternatives Considered" = "Manual copy and paste outside the shared contract or provider-specific core adapters."
            Consequences = "Codex can submit a reviewable proposal without mutating canonical truth."
        }
        links = @{
            dependsOn = @()
            affects = @()
            derivedFrom = @()
            supersedes = @()
        }
        typeSpecificValues = @{
            decision_date = $today
        }
    }
}

$proposal = Invoke-MemoraRequest -Method Post -Uri "$BaseUrl/api/artifacts/proposals" -Body $proposalRequest

$outcomeRequest = @{
    projectId = $ProjectId
    artifactId = $OutcomeArtifactId
    content = @{
        title = "Codex external workflow outcome"
        provenance = "codex"
        reason = "Capture the result of the first Codex external workflow run as a reviewable outcome."
        tags = @("codex", "workflow", "runtime")
        sections = @{
            "What Happened" = "Codex resolved the project, retrieved deterministic context, submitted a proposal, and recorded this outcome through the shared contract."
            Why = "The workflow should stay proposal-only and leave canonical truth unchanged."
            Impact = "The external client loop is now executable without manual context copy and paste."
            "Follow-up" = "Review the draft proposal and recorded outcome through normal Memora operator flows."
        }
        links = @{
            dependsOn = @($ProposalArtifactId)
            affects = @()
            derivedFrom = @()
            supersedes = @()
        }
        typeSpecificValues = @{
            outcome = "success"
        }
    }
}

$outcome = Invoke-MemoraRequest -Method Post -Uri "$BaseUrl/api/outcomes" -Body $outcomeRequest

[pscustomobject]@{
    Project = @{
        ProjectId = $project.projectId
        Name = $project.name
        Status = $project.status
    }
    Context = @{
        TaskDescription = $TaskDescription
        ArtifactIds = $artifactIds
    }
    Proposal = @{
        ArtifactId = $proposal.artifactId
        ResultingStatus = $proposal.resultingStatus
        Revision = $proposal.revision
    }
    Outcome = @{
        ArtifactId = $outcome.artifactId
        ResultingStatus = $outcome.resultingStatus
        Revision = $outcome.revision
        OutcomeKind = $outcome.outcomeKind
    }
}
