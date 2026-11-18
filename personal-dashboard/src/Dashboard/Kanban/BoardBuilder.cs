using PersonalDashboard.Projects;

namespace PersonalDashboard.Kanban;

public sealed class BoardBuilder(LaneAssigner laneAssigner)
{
    public Board Build(IReadOnlyList<ProjectPlan> plans, DateOnly today)
    {
        var ordered = plans
            .OrderBy(plan => plan.Project.DueDate is null)
            .ThenBy(plan => plan.Project.DueDate)
            .ThenBy(plan => plan.Project.Position)
            .ToList();

        var placed = ordered
            .SelectMany(plan => laneAssigner.Assign(plan.Project, plan.Steps, today)
                .Select(placedStep => (plan.Project, placedStep)))
            .ToList();

        var cards = Enum.GetValues<Lane>()
            .SelectMany(lane => Stack(lane, placed.Where(p => p.placedStep.Lane == lane).Select(p => (p.Project, p.placedStep.Step))))
            .ToList();

        return new Board(ordered.Select(plan => plan.Project).ToList(), cards);
    }

    private static IEnumerable<Card> Stack(Lane lane, IEnumerable<(Project Project, Step Step)> stepsInLane)
    {
        var stacks = new List<List<(Project Project, Step Step)>>();
        foreach (var entry in stepsInLane)
        {
            var stack = stacks.FirstOrDefault(s => s[0].Step.Text == entry.Step.Text && s.All(member => member.Project != entry.Project));
            if (stack is null)
            {
                stacks.Add([entry]);
            }
            else
            {
                stack.Add(entry);
            }
        }
        return stacks.Select(stack => new Card(
            lane,
            stack[0].Step.Text,
            stack.Select(member => member.Step.Id).Where(id => id != "").ToList(),
            stack.Select(member => member.Project).ToList()));
    }
}
