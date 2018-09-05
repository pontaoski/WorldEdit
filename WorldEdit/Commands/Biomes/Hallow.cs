using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Hallow : Biome
    {
        public override int Dirt => TileID.Dirt;
        public override int Grass => TileID.HallowedGrass;
        public override int Stone => TileID.Pearlstone;
        public override int Ice => TileID.HallowedIce;
        public override int Clay => TileID.ClayBlock;
        public override int Sand => TileID.Pearlsand;
        public override int HardenedSand => TileID.HallowHardenedSand;
        public override int Sandstone => TileID.HallowSandstone;
        public override int Plants => TileID.HallowedPlants;
        public override int TallPlants => TileID.HallowedPlants2;
        public override int Vines => TileID.HallowedVines;
        public override int Thorn => -1;

        public override byte DirtWall => WallID.Dirt;
        public override byte DirtWallUnsafe => WallID.DirtUnsafe;
        public override byte CaveWall => WallID.CaveWall;
        public override byte DirtWallUnsafe1 => WallID.DirtUnsafe1;
        public override byte DirtWallUnsafe2 => WallID.DirtUnsafe2;
        public override byte DirtWallUnsafe3 => WallID.DirtUnsafe3;
        public override byte DirtWallUnsafe4 => WallID.DirtUnsafe4;
        public override byte StoneWall => WallID.PearlstoneBrickUnsafe;
        public override byte HardenedSandWall => WallID.HallowHardenedSand;
        public override byte SandstoneWall => WallID.HallowSandstone;
        public override byte GrassWall => WallID.HallowedGrassUnsafe;
        public override byte GrassWallUnsafe => WallID.HallowedGrassUnsafe;
        public override byte FlowerWall => WallID.HallowedGrassUnsafe;
        public override byte FlowerWallUnsafe => WallID.HallowedGrassUnsafe;

        public override byte CaveWall1 => WallID.HallowUnsafe1;
        public override byte CaveWall2 => WallID.HallowUnsafe2;
        public override byte CaveWall3 => WallID.HallowUnsafe3;
        public override byte CaveWall4 => WallID.HallowUnsafe4;
    }
}