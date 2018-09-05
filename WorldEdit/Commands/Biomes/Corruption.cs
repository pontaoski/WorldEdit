using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Corruption : Biome
    {
        public override int Dirt => TileID.Dirt;
        public override int Grass => TileID.CorruptGrass;
        public override int Stone => TileID.Ebonstone;
        public override int Ice => TileID.CorruptIce;
        public override int Clay => TileID.ClayBlock;
        public override int Sand => TileID.Ebonsand;
        public override int HardenedSand => TileID.CorruptHardenedSand;
        public override int Sandstone => TileID.CorruptSandstone;
        public override int Plants => TileID.CorruptPlants;
        public override int TallPlants => -1;
        public override int Vines => -1;
        public override int Thorn => TileID.CorruptThorns;

        public override byte DirtWall => WallID.Dirt;
        public override byte DirtWallUnsafe => WallID.DirtUnsafe;
        public override byte CaveWall => WallID.CaveWall;
        public override byte DirtWallUnsafe1 => WallID.DirtUnsafe1;
        public override byte DirtWallUnsafe2 => WallID.DirtUnsafe2;
        public override byte DirtWallUnsafe3 => WallID.DirtUnsafe3;
        public override byte DirtWallUnsafe4 => WallID.DirtUnsafe4;
        public override byte StoneWall => WallID.EbonstoneUnsafe;
        public override byte HardenedSandWall => WallID.CorruptHardenedSand;
        public override byte SandstoneWall => WallID.CorruptSandstone;
        public override byte GrassWall => WallID.CorruptGrassUnsafe;
        public override byte GrassWallUnsafe => WallID.CorruptGrassUnsafe;
        public override byte FlowerWall => WallID.CorruptGrassUnsafe;
        public override byte FlowerWallUnsafe => WallID.CorruptGrassUnsafe;

        public override byte CaveWall1 => WallID.CorruptionUnsafe1;
        public override byte CaveWall2 => WallID.CorruptionUnsafe2;
        public override byte CaveWall3 => WallID.CorruptionUnsafe3;
        public override byte CaveWall4 => WallID.CorruptionUnsafe4;
    }
}