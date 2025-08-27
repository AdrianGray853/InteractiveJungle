
namespace Interactive.PuzzelShape
{
//using EModule = GameDataShape.eGameType;

    // consider implementing a base class for all *Games monobehaviours
    // instead of this interface, as almost all code there is repetitive
using EModule = GameDataShape.eGameType;

    public interface IGameModule
    {
        EModule Type { get; }
        bool    Enabled { set; }
        int     LevelCount { get; }
        void    UpdateFluturiTarget();
    }

}