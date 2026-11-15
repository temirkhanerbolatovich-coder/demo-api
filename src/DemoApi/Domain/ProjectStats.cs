namespace DemoApi.Domain;

/// <summary>Статистика задач проекта.</summary>
/// <param name="ProjectId">Идентификатор проекта.</param>
/// <param name="Total">Всего задач.</param>
/// <param name="Todo">Новые задачи.</param>
/// <param name="InProgress">Задачи в работе.</param>
/// <param name="Done">Выполненные задачи.</param>
/// <param name="Overdue">Незавершённые задачи с истёкшим сроком.</param>
public sealed record ProjectStats(int ProjectId, int Total, int Todo, int InProgress, int Done, int Overdue);
