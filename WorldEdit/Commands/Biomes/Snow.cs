using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Snow : Biome
    {
        public override int Dirt => TileID.SnowBlock;
        public override int Grass => TileID.SnowBlock;
        public override int Stone => TileID.IceBlock;
        public override int Ice => TileID.IceBlock;
        public override int Clay => TileID.SnowBlock;
        public override int Sand => TileID.SnowBlock;
        public override int HardenedSand => TileID.SnowBlock;
        public override int Sandstone => TileID.IceBlock;
        public override int Plants => -1;
        public override int TallPlants => -1;
        public override int Vines => -1;
        public override int Thorn => -1;

        public override byte DirtWall => WallID.SnowWallUnsafe;
        public override byte DirtWallUnsafe => WallID.SnowWallUnsafe;
        public override byte CaveWall => WallID.SnowWallUnsafe;
        public override byte DirtWallUnsafe1 => WallID.SnowWallUnsafe;
        public override byte DirtWallUnsafe2 => WallID.SnowWallUnsafe;
        public override byte DirtWallUnsafe3 => WallID.SnowWallUnsafe;
        public override byte DirtWallUnsafe4 => WallID.SnowWallUnsafe;
        public override byte StoneWall => WallID.IceUnsafe;
        public override byte HardenedSandWall => WallID.IceUnsafe;
        public override byte SandstoneWall => WallID.IceUnsafe;
        public override byte GrassWall => WallID.IceUnsafe;
        public override byte GrassWallUnsafe => WallID.IceUnsafe;
        public override byte FlowerWall => WallID.IceUnsafe;
        public override byte FlowerWallUnsafe => WallID.IceUnsafe;

        public override byte CaveWall1 => 0;
        public override byte CaveWall2 => 0;
        public override byte CaveWall3 => 0;
        public override byte CaveWall4 => 0;
    }
}