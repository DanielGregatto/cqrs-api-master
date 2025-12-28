using System;
using Bogus;
using Services.Features.LogError.Commands.CreateLogError;
using Services.Features.LogError.Commands.UpdateLogError;
using Services.Features.LogError.Commands.DeleteLogError;

namespace Unit.Tests.Fixtures;

public class LogErrorCommandFixture
{
    private readonly Faker<CreateLogErrorCommand> _createCommandFaker;
    private readonly Faker<UpdateLogErrorCommand> _updateCommandFaker;
    private readonly Faker<DeleteLogErrorCommand> _deleteCommandFaker;

    public LogErrorCommandFixture()
    {
        _createCommandFaker = new Faker<CreateLogErrorCommand>()
            .RuleFor(c => c.UserId, f => f.Random.Bool() ? Guid.NewGuid() : (Guid?)null)
            .RuleFor(c => c.Code, f => f.PickRandom("ERR001", "ERR002", "ERR003", "WARN001"))
            .RuleFor(c => c.Record, f => f.Lorem.Paragraph());

        _updateCommandFaker = new Faker<UpdateLogErrorCommand>()
            .RuleFor(c => c.Id, f => Guid.NewGuid())
            .RuleFor(c => c.UserId, f => f.Random.Bool() ? Guid.NewGuid() : (Guid?)null)
            .RuleFor(c => c.Code, f => f.PickRandom("ERR001", "ERR002", "ERR003", "WARN001"))
            .RuleFor(c => c.Record, f => f.Lorem.Paragraph());

        _deleteCommandFaker = new Faker<DeleteLogErrorCommand>()
            .RuleFor(c => c.Id, f => Guid.NewGuid());
    }

    public CreateLogErrorCommand GenerateCreateCommand() => _createCommandFaker.Generate();

    public UpdateLogErrorCommand GenerateUpdateCommand() => _updateCommandFaker.Generate();

    public DeleteLogErrorCommand GenerateDeleteCommand() => _deleteCommandFaker.Generate();
}
