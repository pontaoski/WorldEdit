using OTAPI.Tile;
using System;

namespace WorldEdit.Commands.Biomes
{
    public class Biome
    {
        public virtual int Dirt { get; }
        public virtual int Grass { get; }
        public virtual int Stone { get; }
        public virtual int Ice { get; }
        public virtual int Clay { get; }
        public virtual int Sand { get; }
        public virtual int HardenedSand { get; }
        public virtual int Sandstone { get; }
        public virtual int Plants { get; }
        public virtual int TallPlants { get; }
        public virtual int Vines { get; }
        public virtual int Thorn { get; }

        public int[] Tiles => new int[]
        {
            Dirt, Grass, Stone, Ice, Clay, Sand,
            HardenedSand, Sandstone, Plants,
            TallPlants, Vines, Thorn
        };

        public virtual byte DirtWall { get; }
        public virtual byte StoneWall { get; }
        public virtual byte HardenedSandWall { get; }
        public virtual byte SandstoneWall { get; }
        public virtual byte GrassWall { get; }
        public virtual byte GrassWallUnsafe { get; }
        public virtual byte FlowerWall { get; }
        public virtual byte FlowerWallUnsafe { get; }
        
        public virtual byte CaveWall1 { get; }
        public virtual byte CaveWall2 { get; }
        public virtual byte CaveWall3 { get; }
        public virtual byte CaveWall4 { get; }
        public virtual byte DirtWallUnsafe { get; }
        public virtual byte DirtWallUnsafe1 { get; }
        public virtual byte DirtWallUnsafe2 { get; }
        public virtual byte DirtWallUnsafe3 { get; }
        public virtual byte DirtWallUnsafe4 { get; }
        public virtual byte CaveWall { get; }

        public byte[] Walls => new byte[]
        {
            DirtWall, StoneWall, HardenedSandWall, SandstoneWall,
            GrassWall, GrassWallUnsafe, FlowerWall,
            FlowerWallUnsafe, CaveWall1, CaveWall2, CaveWall3,
            CaveWall4, DirtWallUnsafe, DirtWallUnsafe1,
            DirtWallUnsafe2, DirtWallUnsafe3, DirtWallUnsafe4
        };

        public bool ConvertTile(ITile Tile, Biome ToBiome)
        {
            if (Tile == null) return false;
            bool edited = false;
            if (Tile.active())
            {
                int index = Array.FindIndex(Tiles, t => t == Tile.type);
                if (index >= 0)
                {
                    if (ToBiome.Tiles[index] == -1)
                    {
                        Tile.type = 0;
                        Tile.frameX = -1;
                        Tile.frameY = -1;
                        Tile.active(false);
                    }
                    else { Tile.type = (ushort)ToBiome.Tiles[index]; }
                    edited = true;
                }
            }
            if (Tile.wall > 0)
            {
                int index = Array.FindIndex(Walls, w => w == Tile.wall);
                if (index >= 0 && ToBiome.Walls[index] != 0)
                {
                    Tile.wall = ToBiome.Walls[index];
                    edited = true;
                }
            }
            return edited;
        }
    }
}