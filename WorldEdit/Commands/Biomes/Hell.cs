using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Hell : Biome
    {
        public override int Dirt => TileID.Ash;
        public override int Grass => TileID.Ash;
        public override int Stone => TileID.Hellstone;
        public override int Ice => TileID.Hellstone;
        public override int Clay => TileID.Ash;
        public override int Sand => TileID.Silt;
        public override int HardenedSand => TileID.Ash;
        public override int Sandstone => TileID.Hellstone;
        public override int Plants => -1;
        public override int TallPlants => -1;
        public override int Vines => -1;
        public override int Thorn => -1;

        public override byte DirtWall => 0;
        public override byte DirtWallUnsafe => 0;
        public override byte CaveWall => 0;
        public override byte DirtWallUnsafe1 => 0;
        public override byte DirtWallUnsafe2 => 0;
        public override byte DirtWallUnsafe3 => 0;
        public override byte DirtWallUnsafe4 => 0;
        public override byte StoneWall => 0;
        public override byte HardenedSandWall => 0;
        public override byte SandstoneWall => 0;
        public override byte GrassWall => 0;
        public override byte GrassWallUnsafe => 0;
        public override byte FlowerWall => 0;
        public override byte FlowerWallUnsafe => 0;

        public override byte CaveWall1 => 0;
        public override byte CaveWall2 => 0;
        public override byte CaveWall3 => 0;
        public override byte CaveWall4 => 0;
    }
}