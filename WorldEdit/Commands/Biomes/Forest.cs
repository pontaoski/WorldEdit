using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Forest : Biome
    {
        public override int Dirt => TileID.Dirt;
        public override int Grass => TileID.Grass;
        public override int Stone => TileID.Stone;
        public override int Ice => TileID.IceBlock;
        public override int Clay => TileID.ClayBlock;
        public override int Sand => TileID.Sand;
        public override int HardenedSand => TileID.HardenedSand;
        public override int Sandstone => TileID.Sandstone;
        public override int Plants => TileID.Plants;
        public override int TallPlants => TileID.Plants2;
        public override int Vines => TileID.Vines;
        public override int Thorn => -1;

        public override byte DirtWall => WallID.Dirt;
        public override byte DirtWallUnsafe => WallID.DirtUnsafe;
        public override byte CaveWall => WallID.CaveWall;
        public override byte DirtWallUnsafe1 => WallID.DirtUnsafe1;
        public override byte DirtWallUnsafe2 => WallID.DirtUnsafe2;
        public override byte DirtWallUnsafe3 => WallID.DirtUnsafe3;
        public override byte DirtWallUnsafe4 => WallID.DirtUnsafe4;
        public override byte StoneWall => WallID.Stone;
        public override byte HardenedSandWall => WallID.HardenedSand;
        public override byte SandstoneWall => WallID.Sandstone;
        public override byte GrassWall => WallID.Grass;
        public override byte GrassWallUnsafe => WallID.GrassUnsafe;
        public override byte FlowerWall => WallID.Flower;
        public override byte FlowerWallUnsafe => WallID.FlowerUnsafe;

        public override byte CaveWall1 => 0;
        public override byte CaveWall2 => 0;
        public override byte CaveWall3 => 0;
        public override byte CaveWall4 => 0;
    }
}