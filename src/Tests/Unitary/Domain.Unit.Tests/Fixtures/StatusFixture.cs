using System;
using System.Collections.Generic;
using Bogus;
using Domain;

namespace Unit.Tests.Fixtures;

public class StatusFixture
{
    private readonly Faker<Status> _statusFaker;

    public StatusFixture()
    {
        _statusFaker = new Faker<Status>()
            .RuleFor(s => s.Id, f => Guid.NewGuid())
            .RuleFor(s => s.Escopo, f => f.PickRandom("Geral", "Específico", "Sistema"))
            .RuleFor(s => s.Nome, f => f.Commerce.ProductName())
            .RuleFor(s => s.Descricao, f => f.Lorem.Sentence())
            .RuleFor(s => s.Bloquear, f => f.Random.Bool())
            .RuleFor(s => s.Ativo, f => f.Random.Bool())
            .RuleFor(s => s.Deleted, f => false)
            .RuleFor(s => s.CreatedAt, f => f.Date.Past());
    }

    public Status GenerateStatus() => _statusFaker.Generate();

    public List<Status> GenerateStatusList(int count = 5) => _statusFaker.Generate(count);

    public Status GenerateActiveStatus()
    {
        var status = _statusFaker.Generate();
        status.Ativo = true;
        status.Bloquear = false;
        status.Escopo = "Geral";
        return status;
    }

    public Status GenerateInactiveStatus()
    {
        var status = _statusFaker.Generate();
        status.Ativo = false;
        status.Bloquear = true;
        status.Escopo = "Geral";
        return status;
    }

    public Status GenerateDeletedStatus()
    {
        var status = _statusFaker.Generate();
        status.Deleted = true;
        return status;
    }
}
