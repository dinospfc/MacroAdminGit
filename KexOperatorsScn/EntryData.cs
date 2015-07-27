using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListBoxTest
{
    class EntryData
    {
        //Filler Created because parameters start at 01
        public String parameterVallfiller = null;
        public String parameterNamesfiller = null;

        //BackColor
        public Color backColor = Color.White;

        //All parameters
        public string IDnum;
        public string objName;
        public string funcName;
        public string para01Name = null;
        public string para02Name = null;
        public string para03Name = null;
        public string para04Name = null;
        public string para05Name = null;
        public string para06Name = null;
        public string para07Name = null;
        public string para08Name = null;
        public string para01Val = null;
        public string para02Val = null;
        public string para03Val = null;
        public string para04Val = null;
        public string para05Val = null;
        public string para06Val = null;
        public string para07Val = null;
        public string para08Val = null;

        public string checkNull;

        //Array created to better access the parameters
        public String[] parameterVals = null;
        public String[] parameterNames = null;


        public EntryData()
        {
            parameterVals = new String[]
            {
                parameterVallfiller,
                para01Val,
                para02Val,
                para03Val,
                para04Val,
                para01Val,
                para06Val,
                para07Val,
                para08Val
            };

            parameterNames = new String[]
            {
                parameterNamesfiller,
                para01Name,
                para02Name,
                para03Name,
                para04Name,
                para05Name,
                para06Name,
                para07Name,
                para08Name
            };
        }

    }
}
