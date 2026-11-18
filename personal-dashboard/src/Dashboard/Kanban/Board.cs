using PersonalDashboard.Projects;

namespace PersonalDashboard.Kanban;

public sealed record Board(IReadOnlyList<Project> Projects, IReadOnlyList<Card> Cards)
{
    public IReadOnlyList<Card> CardsIn(Lane lane) => Cards.Where(card => card.Lane == lane).ToList();
}
