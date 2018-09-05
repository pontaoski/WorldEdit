using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Jungle : Biome
    {
        public override int Dirt => TileID.Mud;
        public override int Grass => TileID.JungleGrass;
        public override int Stone => TileID.Stone;
        public override int Ice => TileID.Stone;
        public override int Clay => TileID.ClayBlock;
        public override int Sand => TileID.Sand;
        public override int HardenedSand => TileID.HardenedSand;
        public override int Sandstone => TileID.Sandstone;
        public override int Plants => TileID.JunglePlants;
        public override int TallPlants => TileID.JunglePlants2;
        public override int Vines => TileID.JungleVines;
        public override int Thorn => TileID.JungleThorns;

        public override byte DirtWall => WallID.MudUnsafe;
        public override byte DirtWallUnsafe => WallID.MudUnsafe;
        public override byte CaveWall => WallID.MudUnsafe;
        public override byte DirtWallUnsafe1 => WallID.MudUnsafe;
        public override byte DirtWallUnsafe2 => WallID.MudUnsafe;
        public override byte DirtWallUnsafe3 => WallID.MudUnsafe;
        public override byte DirtWallUnsafe4 => WallID.MudUnsafe;
        public override byte StoneWall => WallID.Stone;
        public override byte HardenedSandWall => WallID.HardenedSand;
        public override byte SandstoneWall => WallID.Sandstone;
        public override byte GrassWall => WallID.Jungle;
        public override byte GrassWallUnsafe => WallID.JungleUnsafe;
        public override byte FlowerWall => WallID.Flower;
        public override byte FlowerWallUnsafe => WallID.FlowerUnsafe;

        public override byte CaveWall1 => WallID.JungleUnsafe1;
        public override byte CaveWall2 => WallID.JungleUnsafe2;
        public override byte CaveWall3 => WallID.JungleUnsafe3;
        public override byte CaveWall4 => WallID.JungleUnsafe4;
    }
}