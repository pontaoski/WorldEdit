using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Desert : Biome
    {
        public override int Dirt => TileID.Sand;
        public override int Clay => TileID.Sand;
        public override int Stone => TileID.Sandstone;
        public override int Ice => TileID.Sandstone;
        public override int Sand => TileID.Sand;
        public override int HardenedSand => TileID.HardenedSand;
        public override int Sandstone => TileID.Sandstone;
        public override int Grass => TileID.Sand;
        public override int Plants => -1;
        public override int TallPlants => -1;
        public override int Vines => -1;
        public override int Thorn => -1;

        public override byte DirtWall => WallID.HardenedSand;
        public override byte DirtWallUnsafe => WallID.HardenedSand;
        public override byte CaveWall => WallID.HardenedSand;
        public override byte DirtWallUnsafe1 => WallID.HardenedSand;
        public override byte DirtWallUnsafe2 => WallID.Sandstone;
        public override byte DirtWallUnsafe3 => WallID.HardenedSand;
        public override byte DirtWallUnsafe4 => WallID.Sandstone;
        public override byte StoneWall => WallID.Sandstone;
        public override byte HardenedSandWall => WallID.HardenedSand;
        public override byte SandstoneWall => WallID.Sandstone;
        public override byte GrassWall => WallID.HardenedSand;
        public override byte GrassWallUnsafe => WallID.HardenedSand;
        public override byte FlowerWall => WallID.HardenedSand;
        public override byte FlowerWallUnsafe => WallID.HardenedSand;

        public override byte CaveWall1 => 0;
        public override byte CaveWall2 => 0;
        public override byte CaveWall3 => 0;
        public override byte CaveWall4 => 0;
    }
}