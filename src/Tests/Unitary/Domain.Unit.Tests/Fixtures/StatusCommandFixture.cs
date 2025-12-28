using System;
using Bogus;
using Services.Features.Status.Commands.CreateStatus;
using Services.Features.Status.Commands.UpdateStatus;
using Services.Features.Status.Commands.DeleteStatus;

namespace Unit.Tests.Fixtures;

public class StatusCommandFixture
{
    private readonly Faker<CreateStatusCommand> _createCommandFaker;
    private readonly Faker<UpdateStatusCommand> _updateCommandFaker;
    private readonly Faker<DeleteStatusCommand> _deleteCommandFaker;

    public StatusCommandFixture()
    {
        _createCommandFaker = new Faker<CreateStatusCommand>()
            .RuleFor(c => c.Escopo, f => f.PickRandom("Geral", "Específico", "Sistema"))
            .RuleFor(c => c.Nome, f => f.Commerce.ProductName())
            .RuleFor(c => c.Descricao, f => f.Lorem.Sentence())
            .RuleFor(c => c.Bloquear, f => f.Random.Bool())
            .RuleFor(c => c.Ativo, f => f.Random.Bool());

        _updateCommandFaker = new Faker<UpdateStatusCommand>()
            .RuleFor(c => c.Id, f => Guid.NewGuid())
            .RuleFor(c => c.Escopo, f => f.PickRandom("Geral", "Específico", "Sistema"))
            .RuleFor(c => c.Nome, f => f.Commerce.ProductName())
            .RuleFor(c => c.Descricao, f => f.Lorem.Sentence())
            .RuleFor(c => c.Bloquear, f => f.Random.Bool())
            .RuleFor(c => c.Ativo, f => f.Random.Bool());

        _deleteCommandFaker = new Faker<DeleteStatusCommand>()
            .RuleFor(c => c.Id, f => Guid.NewGuid());
    }

    public CreateStatusCommand GenerateCreateCommand() => _createCommandFaker.Generate();

    public UpdateStatusCommand GenerateUpdateCommand() => _updateCommandFaker.Generate();

    public DeleteStatusCommand GenerateDeleteCommand() => _deleteCommandFaker.Generate();

    public CreateStatusCommand GenerateInvalidCreateCommand()
    {
        return new CreateStatusCommand
        {
            Escopo = "",
            Nome = "",
            Descricao = new string('a', 501), // Exceeds max length
            Bloquear = false,
            Ativo = true
        };
    }

    public UpdateStatusCommand GenerateInvalidUpdateCommand()
    {
        return new UpdateStatusCommand
        {
            Id = Guid.Empty, // Invalid ID
            Escopo = "",
            Nome = "",
            Descricao = new string('a', 501),
            Bloquear = false,
            Ativo = true
        };
    }
}
