using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Crimson : Biome
    {
        public override int Dirt => TileID.Dirt;
        public override int Grass => TileID.FleshGrass;
        public override int Stone => TileID.Crimstone;
        public override int Ice => TileID.FleshIce;
        public override int Clay => TileID.ClayBlock;
        public override int Sand => TileID.Crimsand;
        public override int HardenedSand => TileID.CrimsonHardenedSand;
        public override int Sandstone => TileID.CrimsonSandstone;
        public override int Plants => TileID.FleshWeeds;
        public override int TallPlants => -1;
        public override int Vines => TileID.CrimsonVines;
        public override int Thorn => TileID.CrimtaneThorns;

        public override byte DirtWall => WallID.Dirt;
        public override byte DirtWallUnsafe => WallID.DirtUnsafe;
        public override byte CaveWall => WallID.CaveWall;
        public override byte DirtWallUnsafe1 => WallID.DirtUnsafe1;
        public override byte DirtWallUnsafe2 => WallID.DirtUnsafe2;
        public override byte DirtWallUnsafe3 => WallID.DirtUnsafe3;
        public override byte DirtWallUnsafe4 => WallID.DirtUnsafe4;
        public override byte StoneWall => WallID.CrimstoneUnsafe;
        public override byte HardenedSandWall => WallID.CrimsonHardenedSand;
        public override byte SandstoneWall => WallID.CrimsonSandstone;
        public override byte GrassWall => WallID.CrimsonGrassUnsafe;
        public override byte GrassWallUnsafe => WallID.CrimsonGrassUnsafe;
        public override byte FlowerWall => WallID.CrimsonGrassUnsafe;
        public override byte FlowerWallUnsafe => WallID.CrimsonGrassUnsafe;

        public override byte CaveWall1 => WallID.CrimsonUnsafe1;
        public override byte CaveWall2 => WallID.CrimsonUnsafe2;
        public override byte CaveWall3 => WallID.CrimsonUnsafe3;
        public override byte CaveWall4 => WallID.CrimsonUnsafe4;
    }
}