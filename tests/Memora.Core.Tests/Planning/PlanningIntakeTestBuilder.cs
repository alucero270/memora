using Memora.Core.Artifacts;
using Memora.Core.Planning;

namespace Memora.Core.Tests.Planning;

internal static class PlanningIntakeTestBuilder
{
    public static PlanningIntake CreateValidIntake() =>
        new(
            new PlanningProjectScope("memora", "Memora"),
            new PlanningSession(
                "Operator planning notes",
                new DateTimeOffset(2026, 04, 15, 10, 30, 00, TimeSpan.Zero),
                "Milestone 2 planning was distilled into draft-ready inputs.",
                ["Review approval workflow dependencies"],
                ["milestone-2", "human-loop"]),
            [
                new PlanDraftSeed(
                    "Define planning intake contract",
                    "Capture structured project planning input before draft generation.",
                    "Limit the first slice to core validation and typed contracts.",
                    ["Validator rejects incomplete intake payloads."],
                    "This slice supports downstream draft generation work.",
                    "Milestone 2 thin slice",
                    ArtifactPriority.High,
                    true,
                    ["planning", "core"])
            ],
            [
                new DecisionDraftSeed(
                    "Keep planning intake in Memora.Core",
                    "Milestone 2 needs a typed contract before storage or UI work starts.",
                    "Introduce the planning intake model in the core domain.",
                    "Push parsing, persistence, and transport concerns to later layers.",
                    "The contract stays reusable across UI, API, and MCP layers.",
                    "Preserve strict module boundaries.",
                    new DateOnly(2026, 04, 15),
                    ["architecture", "core"])
            ],
            [
                new ConstraintDraftSeed(
                    "Filesystem remains canonical",
                    "Planning intake cannot write canonical artifacts directly.",
                    "Memora v1 only allows proposal and draft flows before approval.",
                    "Draft generation must stay separate from filesystem promotion.",
                    "Protect the human approval loop.",
                    ConstraintKind.Workflow,
                    ConstraintSeverity.High,
                    ["governance"])
            ],
            [
                new QuestionDraftSeed(
                    "How should draft ids be assigned?",
                    "Should draft generation allocate ids before persistence?",
                    "Milestone 2 needs draft generation soon after intake validation.",
                    "Evaluate generator-owned ids versus storage-owned ids.",
                    null,
                    "This affects the next thin slice.",
                    QuestionStatus.Open,
                    ArtifactPriority.Normal,
                    ["draft-generation"])
            ]);
}
