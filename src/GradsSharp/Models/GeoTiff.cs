using BitMiracle.LibTiff.Classic;

namespace GradsSharp.Models;

internal enum tagtype_t
{
    TYPE_BYTE=1,
    TYPE_SHORT=2,
    TYPE_LONG=3,
    TYPE_RATIONAL=4,
    TYPE_ASCII=5,
    TYPE_FLOAT=6,
    TYPE_DOUBLE=7,
    TYPE_SBYTE=8,
    TYPE_SSHORT=9,
    TYPE_SLONG=10,
    TYPE_UNKNOWN=11
}
internal enum geokey_t
{
    GTModelTypeGeoKey=1024,	
    GTRasterTypeGeoKey=1025,
    GeographicTypeGeoKey=2048,
}

internal enum rastertype_t
{
    RasterPixelIsArea=1,	// Standard pixel-fills-grid-cell
    RasterPixelIsPoint=2	// Pixel-at-grid-vertex
}

internal enum geographic_t
{
    GCS_WGS_84=4326,
}

internal enum modeltype_t
{
    ModelTypeProjected=1,	// Projection Coordinate System
    ModelTypeGeographic=2,	// Geographic latitude-longitude System
    ModelTypeGeocentric=3,	// Geocentric (X,Y,Z) Coordinate System
    ModelProjected=ModelTypeProjected,		// alias
    ModelGeographic=ModelTypeGeographic,	// alias
    ModelGeocentric=ModelTypeGeocentric		// alias
}

internal class GeoKey
{
    public geokey_t gk_key;		// GeoKey ID
    public tagtype_t gk_type;	// TIFF data type
    public int gk_count;		// number of values
    public object gk_data;		// pointer to data, or value
}

internal class GeoTiff
{
	
	const int GvCurrentVersion=1;
	const int GvCurrentRevision=1; 
	const int GvCurrentMinorRev=0;	
	
    private Tiff tif;
    
    internal Dictionary<geokey_t, GeoKey> gt_keys = new();

    public GeoTiff(Tiff tif)
    {
        this.tif = tif;
    }

    public bool GTIFKeySet(geokey_t keyID, modeltype_t modelType)
    {
        ushort[] val1=new ushort[] { (ushort)modelType };
        return GTIFKeySet(keyID, val1);
    }
    
    public bool GTIFKeySet(geokey_t keyID, rastertype_t rastertype)
    {
        ushort[] val1=new ushort[] { (ushort)rastertype };
        return GTIFKeySet(keyID, val1);
    }    
    
    public bool GTIFKeySet(geokey_t keyID, geographic_t geographic)
    {
        ushort[] val1=new ushort[] { (ushort)geographic };
        return GTIFKeySet(keyID, val1);
    }
    public bool GTIFKeySet(geokey_t keyID, ushort[]? val)
    {
        if(val==null) // delete the indicated tag
        {
            if(!gt_keys.ContainsKey(keyID)) return false;
            gt_keys.Remove(keyID);
            //gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
            return true;
        }

        if(gt_keys.ContainsKey(keyID))
        {
            gt_keys.Remove(keyID);
            //gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
        }

        // We need to create the key
        try
        {
            GeoKey key=new GeoKey();
            key.gk_key=keyID;
            key.gk_type=tagtype_t.TYPE_SHORT;
            key.gk_count=val.Length;

            ushort[] tmp=new ushort[val.Length];
            val.CopyTo(tmp, 0);
            key.gk_data=tmp;

            gt_keys.Add(keyID, key);
            //gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
            return true;
        }
        catch
        {
            return false;
        }
    }
    public bool GTIFWriteKeys()
		{
			//if((gt.gt_flags&gtiff_flags.FLAG_FILE_MODIFIED)==0) return true;
			//if(gt.gt_tif==null) return false;

			List<geokey_t> keys=new List<geokey_t>(gt_keys.Keys);
			keys.Sort();

			List<ushort> shorts=new List<ushort>();
			List<ushort> shortsValues=new List<ushort>();
			List<double> doubles=new List<double>();
			string strings="";

			// Set up header of ProjectionInfo tag
			shorts.Add((ushort)GvCurrentVersion);
			shorts.Add((ushort)GvCurrentRevision);
			shorts.Add((ushort)GvCurrentMinorRev);
			shorts.Add((ushort)keys.Count);

			int shortOffset=4+keys.Count*4;

			foreach(geokey_t key in keys)
			{
				GeoKey keyptr=gt_keys[key];
				if(keyptr.gk_type==tagtype_t.TYPE_ASCII)
				{
					string str=keyptr.gk_data as string;
					if(str==null) str="";

					str=str.Trim('\0');
					str+="|";
					str=str.Replace('\0', '|');

					shorts.Add((ushort)key);
					shorts.Add((ushort)TiffTag.GEOTIFF_GEOASCIIPARAMSTAG);
					shorts.Add((ushort)str.Length);
					shorts.Add((ushort)strings.Length);
					strings+=str;

					continue;
				}

				if(keyptr.gk_type==tagtype_t.TYPE_DOUBLE)
				{
					double[] dbl=keyptr.gk_data as double[];
					if(dbl==null)
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG);
						shorts.Add((ushort)0);
						shorts.Add((ushort)0);
					}
					else
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG);
						shorts.Add((ushort)dbl.Length);
						shorts.Add((ushort)doubles.Count);
						doubles.AddRange(dbl);
					}
					continue;
				}

				ushort[] sht=keyptr.gk_data as ushort[];
				if(sht==null)
				{
					shorts.Add((ushort)key);
					shorts.Add((ushort)0);
					shorts.Add((ushort)0);
					shorts.Add((ushort)0);
				}
				else
				{
					if(sht.Length<2)
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)0);
						shorts.Add((ushort)sht.Length);
						if(sht.Length==1) shorts.Add(sht[0]);
						else shorts.Add(0);
					}
					else
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
						shorts.Add((ushort)sht.Length);
						shorts.Add((ushort)(shortOffset+shortsValues.Count));
						shortsValues.AddRange(sht);
					}
				}
			}

			if(shorts.Count!=shortOffset) return false;
			shorts.AddRange(shortsValues);

			// Write out the Key Directory
			tif.SetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG, shorts.ToArray());
			
			// Write out the params directories
			if(doubles.Count>0) tif.SetField(TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG, doubles.ToArray());
			if(strings.Length>0) tif.SetField(TiffTag.GEOTIFF_GEOASCIIPARAMSTAG, strings);
			
			//gt.gt_flags&=~gtiff_flags.FLAG_FILE_MODIFIED;

			return true;
		}
}