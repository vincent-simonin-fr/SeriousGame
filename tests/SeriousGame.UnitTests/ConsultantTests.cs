using Server.Domain;

namespace SeriousGame.UnitTests;

public class ConsultantTests
{
    private static Consultant MakeConsultant() => new()
    {
        Firstname = "Ada",
        Lastname = "Lovelace",
        Company = new Company { Name = "Test Co", PlayerOwner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "c1" } }
    };

    [Fact]
    public void SetSalaryRequirement_UpdatesValue()
    {
        var consultant = MakeConsultant();

        consultant.SetSalaryRequirement(4000);

        Assert.Equal(4000, consultant.SalaryRequirement);
    }

    [Fact]
    public void SetSalaryRequirement_NegativeAmount_Throws()
    {
        var consultant = MakeConsultant();

        Assert.Throws<ArgumentOutOfRangeException>(() => consultant.SetSalaryRequirement(-1));
    }
}
