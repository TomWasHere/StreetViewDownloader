using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetviewDownloader
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class panorama
    {

        private panoramaData_properties data_propertiesField;

        private panoramaProjection_properties projection_propertiesField;

        private panoramaLink[] annotation_propertiesField;

        /// <remarks/>
        public panoramaData_properties data_properties
        {
            get
            {
                return this.data_propertiesField;
            }
            set
            {
                this.data_propertiesField = value;
            }
        }

        /// <remarks/>
        public panoramaProjection_properties projection_properties
        {
            get
            {
                return this.projection_propertiesField;
            }
            set
            {
                this.projection_propertiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("link", IsNullable = false)]
        public panoramaLink[] annotation_properties
        {
            get
            {
                return this.annotation_propertiesField;
            }
            set
            {
                this.annotation_propertiesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class panoramaData_properties
    {

        private string copyrightField;

        private string textField;

        private string regionField;

        private string countryField;

        private ushort image_widthField;

        private ushort image_heightField;

        private ushort tile_widthField;

        private ushort tile_heightField;

        private string image_dateField;

        private string pano_idField;

        private byte num_zoom_levelsField;

        private decimal latField;

        private decimal lngField;

        private decimal original_latField;

        private decimal original_lngField;

        private decimal elevation_wgs84_mField;

        /// <remarks/>
        public string copyright
        {
            get
            {
                return this.copyrightField;
            }
            set
            {
                this.copyrightField = value;
            }
        }

        /// <remarks/>
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public string region
        {
            get
            {
                return this.regionField;
            }
            set
            {
                this.regionField = value;
            }
        }

        /// <remarks/>
        public string country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort image_width
        {
            get
            {
                return this.image_widthField;
            }
            set
            {
                this.image_widthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort image_height
        {
            get
            {
                return this.image_heightField;
            }
            set
            {
                this.image_heightField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort tile_width
        {
            get
            {
                return this.tile_widthField;
            }
            set
            {
                this.tile_widthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort tile_height
        {
            get
            {
                return this.tile_heightField;
            }
            set
            {
                this.tile_heightField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "gYearMonth")]
        public string image_date
        {
            get
            {
                return this.image_dateField;
            }
            set
            {
                this.image_dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string pano_id
        {
            get
            {
                return this.pano_idField;
            }
            set
            {
                this.pano_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte num_zoom_levels
        {
            get
            {
                return this.num_zoom_levelsField;
            }
            set
            {
                this.num_zoom_levelsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal lat
        {
            get
            {
                return this.latField;
            }
            set
            {
                this.latField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal lng
        {
            get
            {
                return this.lngField;
            }
            set
            {
                this.lngField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal original_lat
        {
            get
            {
                return this.original_latField;
            }
            set
            {
                this.original_latField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal original_lng
        {
            get
            {
                return this.original_lngField;
            }
            set
            {
                this.original_lngField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal elevation_wgs84_m
        {
            get
            {
                return this.elevation_wgs84_mField;
            }
            set
            {
                this.elevation_wgs84_mField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class panoramaProjection_properties
    {

        private string projection_typeField;

        private decimal pano_yaw_degField;

        private decimal tilt_yaw_degField;

        private decimal tilt_pitch_degField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string projection_type
        {
            get
            {
                return this.projection_typeField;
            }
            set
            {
                this.projection_typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal pano_yaw_deg
        {
            get
            {
                return this.pano_yaw_degField;
            }
            set
            {
                this.pano_yaw_degField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal tilt_yaw_deg
        {
            get
            {
                return this.tilt_yaw_degField;
            }
            set
            {
                this.tilt_yaw_degField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal tilt_pitch_deg
        {
            get
            {
                return this.tilt_pitch_degField;
            }
            set
            {
                this.tilt_pitch_degField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class panoramaLink
    {

        private string link_textField;

        private decimal yaw_degField;

        private string pano_idField;

        private string road_argbField;

        private byte sceneField;

        /// <remarks/>
        public string link_text
        {
            get
            {
                return this.link_textField;
            }
            set
            {
                this.link_textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal yaw_deg
        {
            get
            {
                return this.yaw_degField;
            }
            set
            {
                this.yaw_degField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string pano_id
        {
            get
            {
                return this.pano_idField;
            }
            set
            {
                this.pano_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string road_argb
        {
            get
            {
                return this.road_argbField;
            }
            set
            {
                this.road_argbField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte scene
        {
            get
            {
                return this.sceneField;
            }
            set
            {
                this.sceneField = value;
            }
        }
    }
}
