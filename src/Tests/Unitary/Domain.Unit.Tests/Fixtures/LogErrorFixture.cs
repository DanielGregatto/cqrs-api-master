using System;
using System.Collections.Generic;
using Bogus;
using Domain;

namespace Unit.Tests.Fixtures;

public class LogErrorFixture
{
    private readonly Faker<LogError> _logErrorFaker;

    public LogErrorFixture()
    {
        _logErrorFaker = new Faker<LogError>()
            .RuleFor(l => l.Id, f => Guid.NewGuid())
            .RuleFor(l => l.UserId, f => f.Random.Bool() ? Guid.NewGuid() : (Guid?)null)
            .RuleFor(l => l.Code, f => f.PickRandom("ERR001", "ERR002", "ERR003", "WARN001", "INFO001"))
            .RuleFor(l => l.Record, f => f.Lorem.Paragraph())
            .RuleFor(l => l.Deleted, f => false)
            .RuleFor(l => l.CreatedAt, f => f.Date.Past());
    }

    public LogError GenerateLogError() => _logErrorFaker.Generate();

    public List<LogError> GenerateLogErrorList(int count = 5) => _logErrorFaker.Generate(count);

    public LogError GenerateDeletedLogError()
    {
        var logError = _logErrorFaker.Generate();
        logError.Deleted = true;
        return logError;
    }
}
