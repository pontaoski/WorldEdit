using System;
using System.Linq;
using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
    public class Shape : WECommand
    {
        private Expression expression;
        private int shapeType;
        private int rotateType;
        private int flipType;
        private PlaceID what;
        private bool filled;

        public Shape(int x, int y, int x2, int y2, MagicWand magicWand, TSPlayer plr,
            int shapeType, int rotateType, int flipType, PlaceID what, bool filled,
            Expression expression)
            : base(x, y, x2, y2, magicWand, plr, false)
        {
            this.expression = expression ?? new TestExpression(new Test(t => true));
            this.shapeType = shapeType;
            this.rotateType = rotateType;
            this.flipType = flipType;
            this.what = what;
            this.filled = filled;
        }

        public override void Execute()
        {
            if (!CanUseCommand()) { return; }
            if (shapeType != 0)
            {
                Position(true);
                Tools.PrepareUndo(x, y, x2, y2, plr);
            }
            else
            {
                Tools.PrepareUndo(Math.Min(x, x2), Math.Min(y, y2),
                    Math.Max(x, x2), Math.Max(y, y2), plr);
            }
            int edits = 0;
            var set = (int x, int y, PlaceID what) =>
            {
                switch (what)
                {
                    case TilePlaceID tileID:
                        {
                            if (tileID.CanSet(Main.tile[x, y],
                                select, expression, magicWand, x, y, plr))
                            {
                                tileID.SetTile(x, y);
                                edits++;
                            }
                            break;
                        }
                    case WallPlaceID wallID:
                        {
                            if (wallID.CanSet(Main.tile[x, y],
                                select, expression, magicWand, x, y, plr))
                            {
                                Main.tile[x, y].wall = (ushort)wallID.wallID;
                                edits++;
                            }
                            break;
                        }
                }
            };
            switch (shapeType)
            {
                #region Line

                case 0:
                    {
                        WEPoint[] points = Tools.CreateLine(x, y, x2, y2);
                        points.ForEach(p => set(p.X, p.Y, what));
                        break;
                    }

                #endregion
                #region Rectangle

                case 1:
                    {
                        for (int i = x; i <= x2; i++)
                        {
                            for (int j = y; j <= y2; j++)
                            {
                                bool rectangleBorder = i == x || i == x2 || j == y || j == y2;
                                if (filled || rectangleBorder)
                                {
                                    set(i, j, what);
                                }
                            }
                        }
                        break;
                    }

                #endregion
                #region Ellipse

                case 2:
                    {
                        if (filled) {
                            for (int i = x; i <= x2; i++)
                            {
                                for (int j = y; j <= y2; j++)
                                {
                                    bool inEllipse =
                                        Tools.InEllipse(Math.Min(x, x2),
                                            Math.Min(y, y2), Math.Max(x, x2),
                                            Math.Max(y, y2), i, j);

                                    if (inEllipse)
                                    {
                                        set(i, j, what);
                                    }
                                }
                            }
                        } else {
                            WEPoint[] points = Tools.CreateEllipseOutline(x, y, x2, y2);
                            foreach (WEPoint p in points)
                            {
                                set(p.X, p.Y, what);
                            }
                        }
                        break;
                    }

                #endregion
                #region IsoscelesTriangle, RightTriangle

                case 3:
                case 4:
                    {
                        WEPoint[] points, line1, line2;
                        if (shapeType == 3)
                        {
                            switch (rotateType)
                            {
                                #region Up

                                case 0:
                                    {
                                        int center = x + ((x2 - x) / 2);
                                        points = Tools.CreateLine(center, y, x, y2)
                                         .Concat(Tools.CreateLine(center + ((x2 - x) % 2), y, x2, y2))
                                         .ToArray();
                                        line1 = new WEPoint[]
                                        {
                                            new WEPoint((short)x, (short)y2),
                                            new WEPoint((short)x2, (short)y2)
                                        };
                                        line2 = null;
                                        break;
                                    }

                                #endregion
                                #region Down

                                case 1:
                                    {
                                        int center = x + ((x2 - x) / 2);
                                        points = Tools.CreateLine(center, y2, x, y)
                                         .Concat(Tools.CreateLine(center + ((x2 - x) % 2), y2, x2, y))
                                         .ToArray();
                                        line1 = new WEPoint[]
                                        {
                                            new WEPoint((short)x, (short)y),
                                            new WEPoint((short)x2, (short)y)
                                        };
                                        line2 = null;
                                        break;
                                    }

                                #endregion
                                #region Left

                                case 2:
                                    {
                                        int center = y + ((y2 - y) / 2);
                                        points = Tools.CreateLine(x, center, x2, y)
                                         .Concat(Tools.CreateLine(x, center + ((y2 - y) % 2), x2, y2))
                                         .ToArray();
                                        line1 = new WEPoint[]
                                        {
                                            new WEPoint((short)x2, (short)y),
                                            new WEPoint((short)x2, (short)y2)
                                        };
                                        line2 = null;
                                        break;
                                    }

                                #endregion
                                #region Right

                                case 3:
                                    {
                                        int center = y + ((y2 - y) / 2);
                                        points = Tools.CreateLine(x2, center, x, y)
                                         .Concat(Tools.CreateLine(x2, center + ((x2 - x) % 2), x, y2))
                                         .ToArray();
                                        line1 = new WEPoint[]
                                        {
                                            new WEPoint((short)x, (short)y),
                                            new WEPoint((short)x, (short)y2)
                                        };
                                        line2 = null;
                                        break;
                                    }

                                #endregion
                                default: return;
                            }
                        }
                        else
                        {
                            switch (flipType)
                            {
                                #region Left

                                case 0:
                                    {
                                        switch (rotateType)
                                        {
                                            #region Up

                                            case 0:
                                                {
                                                    points = Tools.CreateLine(x, y2, x2, y);
                                                    line1 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x, (short)y2),
                                                        new WEPoint((short)x2, (short)y2)
                                                    };
                                                    line2 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x2, (short)y),
                                                        new WEPoint((short)x2, (short)y2)
                                                    };
                                                    break;
                                                }

                                            #endregion
                                            #region Down

                                            case 1:
                                                {
                                                    points = Tools.CreateLine(x, y, x2, y2);
                                                    line1 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x, (short)y),
                                                        new WEPoint((short)x2, (short)y)
                                                    };
                                                    line2 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x2, (short)y),
                                                        new WEPoint((short)x2, (short)y2)
                                                    };
                                                    break;
                                                }

                                            #endregion
                                            default: return;
                                        }
                                        break;
                                    }

                                #endregion
                                #region Right

                                case 1:
                                    {
                                        switch (rotateType)
                                        {
                                            #region Up

                                            case 0:
                                                {
                                                    points = Tools.CreateLine(x, y, x2, y2);
                                                    line1 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x, (short)y),
                                                        new WEPoint((short)x, (short)y2)
                                                    };
                                                    line2 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x, (short)y2),
                                                        new WEPoint((short)x2, (short)y2)
                                                    };
                                                    break;
                                                }

                                            #endregion
                                            #region Down

                                            case 1:
                                                {
                                                    points = Tools.CreateLine(x, y2, x2, y);
                                                    line1 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x, (short)y),
                                                        new WEPoint((short)x, (short)y2)
                                                    };
                                                    line2 = new WEPoint[]
                                                    {
                                                        new WEPoint((short)x, (short)y),
                                                        new WEPoint((short)x2, (short)y)
                                                    };
                                                    break;
                                                }

                                            #endregion
                                            default: return;
                                        }
                                        break;
                                    }

                                #endregion
                                default: return;
                            }
                        }

                        if (filled)
                        {
                            switch (rotateType)
                            {
                                #region Up

                                case 0:
                                    {
                                        foreach (WEPoint p in points)
                                        {
                                            for (int y = p.Y; y <= y2; y++)
                                            {
                                                set(p.X, y, what);
                                            }
                                        }
                                        break;
                                    }

                                #endregion
                                #region Down

                                case 1:
                                    {
                                        foreach (WEPoint p in points)
                                        {
                                            for (int y = p.Y; y >= this.y; y--)
                                            {
                                                set(p.X, y, what);
                                            }
                                        }
                                        break;
                                    }

                                #endregion
                                #region Left

                                case 2:
                                    {
                                        foreach (WEPoint p in points)
                                        {
                                            for (int x = p.X; x <= x2; x++)
                                            {
                                                set(x, p.Y, what);
                                            }
                                        }
                                        break;
                                    }

                                #endregion
                                #region Right

                                case 3:
                                    {
                                        foreach (WEPoint p in points)
                                        {
                                            for (int x = p.X; x >= this.x; x--)
                                            {
                                                set(x, p.Y, what);
                                            }
                                        }
                                        break;
                                    }

                                #endregion
                                default: return;
                            }
                        }
                        else
                        {
                            foreach (WEPoint p in points)
                            {
                                set(p.X, p.Y, what);
                            }
                            for (int x = line1[0].X; x <= line1[1].X; x++)
                            {
                                for (int y = line1[0].Y; y <= line1[1].Y; y++)
                                {
                                    set(x, y, what);
                                }
                            }
                            if (line2 != null)
                            {
                                for (int x = line2[0].X; x <= line2[1].X; x++)
                                {
                                    for (int y = line2[0].Y; y <= line2[1].Y; y++)
                                    {
                                        set(x, y, what);
                                    }
                                }
                            }
                        }

                        break;
                    }

                    #endregion
            }

            ResetSection();
            plr.SendSuccessMessage("Set {0}{1} shape. ({2})", filled ? "filled " : "", what.Name, edits);
        }
    }
}