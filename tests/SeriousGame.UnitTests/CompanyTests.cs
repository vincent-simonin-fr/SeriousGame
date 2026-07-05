using Server.Domain;

namespace SeriousGame.UnitTests;

public class CompanyTests
{
    private static Company MakeCompany() => new()
    {
        Name = "Test Co",
        PlayerOwner = new Player { Id = "p1", Nickname = "Owner", ConnectionId = "c1" }
    };

    [Fact]
    public void Deposit_IncreasesTreasury()
    {
        var company = MakeCompany();
        var before = company.Treasury;

        company.Deposit(500);

        Assert.Equal(before + 500, company.Treasury);
    }

    [Fact]
    public void Withdraw_DecreasesTreasury()
    {
        var company = MakeCompany();
        var before = company.Treasury;

        company.Withdraw(500);

        Assert.Equal(before - 500, company.Treasury);
    }

    [Fact]
    public void Deposit_NegativeAmount_Throws()
    {
        var company = MakeCompany();

        Assert.Throws<ArgumentOutOfRangeException>(() => company.Deposit(-1));
    }

    [Fact]
    public void Withdraw_NegativeAmount_Throws()
    {
        var company = MakeCompany();

        Assert.Throws<ArgumentOutOfRangeException>(() => company.Withdraw(-1));
    }
}
