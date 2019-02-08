using EF.Demo.Test;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var test = new GeometryTest();
        await test.Test();
    }
}