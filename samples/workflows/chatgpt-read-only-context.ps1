param(
    [string]$BaseUrl = "http://127.0.0.1:5081",
    [string]$ProjectId = "demo-project",
    [string]$TaskDescription = "Validate the shared read-only state view for ChatGPT-oriented workflows."
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

[pscustomobject]@{
    Project = @{
        ProjectId = $project.projectId
        Name = $project.name
        Status = $project.status
    }
    Context = @{
        TaskDescription = $TaskDescription
        ArtifactIds = @(
            $context.bundle.layers |
                ForEach-Object { $_.artifacts } |
                ForEach-Object { $_.artifact.id }
        ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    }
}
