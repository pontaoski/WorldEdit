using Terraria.ID;
namespace WorldEdit.Commands.Biomes
{
    public class Mushroom : Biome
    {
        public override int Dirt => TileID.Mud;
        public override int Grass => TileID.MushroomGrass;
        public override int Stone => TileID.Stone;
        public override int Ice => TileID.Stone;
        public override int Clay => TileID.ClayBlock;
        public override int Sand => TileID.Mud;
        public override int HardenedSand => TileID.Mud;
        public override int Sandstone => TileID.Stone;
        public override int Plants => TileID.MushroomPlants;
        public override int TallPlants => -1;
        public override int Vines => -1;
        public override int Thorn => -1;

        public override byte DirtWall => WallID.Mushroom;
        public override byte DirtWallUnsafe => WallID.Mushroom;
        public override byte CaveWall => WallID.Mushroom;
        public override byte DirtWallUnsafe1 => WallID.Mushroom;
        public override byte DirtWallUnsafe2 => WallID.Mushroom;
        public override byte DirtWallUnsafe3 => WallID.Mushroom;
        public override byte DirtWallUnsafe4 => WallID.Mushroom;
        public override byte StoneWall => WallID.Stone;
        public override byte HardenedSandWall => WallID.Mushroom;
        public override byte SandstoneWall => WallID.Mushroom;
        public override byte GrassWall => WallID.Mushroom;
        public override byte GrassWallUnsafe => WallID.MushroomUnsafe;
        public override byte FlowerWall => WallID.Mushroom;
        public override byte FlowerWallUnsafe => WallID.MushroomUnsafe;

        public override byte CaveWall1 => 0;
        public override byte CaveWall2 => 0;
        public override byte CaveWall3 => 0;
        public override byte CaveWall4 => 0;
    }
}