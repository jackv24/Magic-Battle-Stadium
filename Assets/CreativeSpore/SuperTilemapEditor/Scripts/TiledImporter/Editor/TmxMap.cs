using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CreativeSpore.TiledImporter
{
    [XmlRoot("map")]
    public class TmxMap
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("orientation")]
        public string Orientation { get; set; }

        [XmlAttribute("tilewidth")]
        public string TileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public string TileHeight { get; set; }

        [XmlElement(Order = 0, ElementName="tileset")]
        public List<TmxTileset> Tilesets { get; set; }

        [XmlElement(Order = 1, ElementName = "layer")]
        public List<TmxLayer> Layers { get; set; }

        public TmxMap()
        {
            Tilesets = new List<TmxTileset>();
            Layers = new List<TmxLayer>();
        }

        internal void FixExportedTilesets(string relativePath)
        {
            XMLSerializer objSerializer = new XMLSerializer();
            for(int i = 0; i < Tilesets.Count; ++i)
            {
                if(!string.IsNullOrEmpty(Tilesets[i].Source))
                {
                    int firstGid = Tilesets[i].FirstGId;
                    Tilesets[i] = objSerializer.LoadFromXMLFile<TmxTileset>( Path.Combine( relativePath, Tilesets[i].Source));
                    Tilesets[i].FirstGId = firstGid;
                }
                if (Tilesets[i].TileCount == 0)
                {
                    int horTiles = System.Convert.ToInt32(Math.Round((float)(Tilesets[i].Image.Width - 2 * Tilesets[i].Margin) / (Tilesets[i].TileWidth + Tilesets[i].Spacing)));
                    int verTiles = System.Convert.ToInt32(Math.Round((float)(Tilesets[i].Image.Height - 2 * Tilesets[i].Margin) / (Tilesets[i].TileHeight + Tilesets[i].Spacing)));
                    Tilesets[i].Columns = horTiles;
                    Tilesets[i].TileCount = horTiles * verTiles;
                }
            }
        }
    }
}
