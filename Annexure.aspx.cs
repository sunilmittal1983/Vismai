using Vismai.Utilities;
using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mysore
{
    public partial class Annexure : System.Web.UI.Page
    {
        string DeptID;
        DataTable dt = new DataTable();
        string DeptName, District, DDLCode;
        string ChallanRefNumber;
        SqlConnection con;
        protected void Page_Load(object sender, EventArgs e) { 


             LoadData();
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                Response.Redirect("Login.aspx");
               
            }
        }

        private void LoadData()
        {
            //DeptID = Session["DeptID"].ToString();
            DeptID = Session["DeptID"] == null ? "1" : Session["DeptID"].ToString();
            con = Repository.GetConnection();
            con.Open();
            string commandtext = "Select DDOCode, DeptName,District from Deptartment where DeptId='" + DeptID + "' ";
            SqlCommand sqlCommandText = new SqlCommand(commandtext, con);
            SqlDataReader reader = sqlCommandText.ExecuteReader();
            while (reader.Read())
            {
                DDLCode = reader[0].ToString();
                DeptName = reader[1].ToString();
                District = reader[2].ToString();
            }
            reader.Close();
            con.Close();
        }

        private void BindMainRepeater()
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                ChallanRefNumber = String.Format("{0}", Request.Form["tbname"]);
                string StartDate = String.Format("{0}", Request.Form["bdate"]);
                string EndDate = String.Format("{0}", Request.Form["edate"]);
                StringBuilder str = new StringBuilder();
                str.Append(@"select 1, A.Challan_creation_Date,A.BankName,A.DemandDraftNumber,A.DDDate,case when A.Type_of_Payment='DD' then A.Challan_Amount else 0 end as 'DDAmount', 
case when A.Type_of_Payment='Cash' then A.Challan_Amount else 0 end as 'CashAmount', 
case when A.Type_of_Payment='K2' then A.Challan_Amount else 0 end As 'K2Amount', 
B.Challan_RefNo,A.Challan_Remit_Date, B.ChallanId, B.Challan_number, B.Challan_ScrollDate, DATEDIFF(day,A.Challan_Remit_Date,B.Challan_ScrollDate) As 'Realisation_Days', A.Date_of_Registration, A.Type_of_Doc, A.Challan_Document_No from Challan A inner join ImportTreasury B on A.Challan_Ref = B.Challan_RefNo  where A.Head_of_Account = '" + Select1.Items[Select1.SelectedIndex].Value.ToString() + "' AND FORMAT(CAST(A.Date_of_Registration as datetime2),'YYYY-MM-DD')  between FORMAT(CAST('" + StartDate + "' as Datetime2),'YYYY-MM-DD') AND FORMAT( CAST('" + EndDate + "' as Datetime2) ,'YYYY-MM-DD')");
                if (Select1.Items[Select1.SelectedIndex].Text.ToString() == "Duty")
                    str.Append("AND B.Head_of_Account in ('0030~02~102~0~01~000','0030~02~103~0~01~000')");
                else
                    str.Append("AND B.Head_of_Account = '" + Select1.Items[Select1.SelectedIndex].Value.ToString().Replace('-', '~') + "'");
                cmd = new SqlCommand(str.ToString(), Repository.GetConnection());
                cmd.Connection.Open();
                SqlDataAdapter ad = new SqlDataAdapter(cmd);

                ad.SelectCommand = cmd;
                ad.Fill(dt);
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                cmd.Connection.Close();
                cmd.Connection.Dispose();
            }
        }


        protected void Repeater1_ItemCommand(object source, RepeaterCommandEventArgs e)
        {           
           
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            BindMainRepeater();

            string StartDate = String.Format("{0}", Request.Form["bdate"]);
            string EndDate = String.Format("{0}", Request.Form["edate"]);
            StartDate = Convert.ToDateTime(StartDate).ToString("dd/MM/yyyy");
            EndDate=Convert.ToDateTime(EndDate).ToString("dd/MM/yyyy");
            StringBuilder strHTMLBuilder = new StringBuilder();

            strHTMLBuilder.Append("<html >");

            strHTMLBuilder.Append("<head>");

            strHTMLBuilder.Append("</head>");

            strHTMLBuilder.Append("<body>");

            strHTMLBuilder.Append("<div align='center' id='printarea'></br><table width='100 % ' align='center' border='0'><tr><td align='center'><img src='D:/Mysore/Mysore/images/GOK_logo.png'  height='44' width='44' ></td></tr></table><p align='center' style='margin: 0;padding: 0; font-size:22'><b>Vismai</b></p><p align='center' style='margin: 0;padding: 0;'>Department of Stamps & Registration</p><p align='center' style='margin: 0;padding: 0; font-size:20'><b>" + DeptName + ", " + District + "</b></p>");

            // strHTMLBuilder.Append("<table align='center' width='100 % '><tr><td style='font-size:12;padding-center:24px'>DDO Office : " + DeptName + " </td><td align ='center'></td ><td ></td></tr><tr><td style='font-size:12;padding-left:24px'>DDO Code :  " + DDLCode+" </td><td></td><td></td></tr><tr><td align ='center' colspan='3'><h5 align='center'> ANNEXURE - A(Duty) HoA: '" + String.Format("{0}", Request.Form["tbname"]) + "' </ h5 ></td></tr></table>");
            strHTMLBuilder.Append(@"<table align='center' width='100%'><tr><td style='font-size:12; padding-left:24px'>DDO Office : ");
            strHTMLBuilder.Append(DeptName);
            strHTMLBuilder.Append(@"</td><td align='center'></td><td></td></tr> <tr><td style='font-size:12; padding-left:24px'>DDO Code :");
            strHTMLBuilder.Append(DDLCode);
            strHTMLBuilder.Append(@"</td><td></td><td></td></tr><tr><td style='font-size:12; padding-left:24px'>From Date: " + StartDate +"</td><td align='center' colspan='3'><h5 align='center'>ANNEXURE-A( " + Select1.Items[Select1.SelectedIndex].Text.ToString() + ") &nbsp;HoA: " + Select1.Items[Select1.SelectedIndex].Value.ToString() + "  </h5> </td><td align='right' style='font-size:12; padding-left:24px'>To Date: " + EndDate+"</td></tr></table>");

            strHTMLBuilder.Append(@"<table border='1' cellspacing='0' cellpadding='0' align='center' style='text-align:center;font-size:12'>

        <thead>

            <tr>

                <th rowspan='2' align='center'>Sl No.</th>

                <th rowspan='2' align='center'>CHALLAN CREATED DATE</th>

                <th colspan='3'align='center'>DD/PAY-ORDER DETAILS</th>

                <th colspan='3'align='center'>AMOUNT REMITTED</th>

                <th rowspan='2'align='center'>K2 CHALLAN RF NO</th>

                <th rowspan='2'>DAT OF REMITTANCE TO BANK</th>

                <th rowspan='2'>KTC-25 serial NO</th>

                <th rowspan='2'>KTC -25 CHALLAN NO</th>

                <th rowspan='2'>DATE OF REALISATION (scroll date)</th>

                <th rowspan='2'>REALISATION DAYS</th>

                <th rowspan='2'>DATE OF REGISTRATION </th>");

            if (Select1.Items[Select1.SelectedIndex].Text.ToString().Contains("Duty") || Select1.Items[Select1.SelectedIndex].Text.ToString().Contains("Registr") ||

             Select1.Items[Select1.SelectedIndex].Text.ToString().Contains("Cess"))

            {

                strHTMLBuilder.Append(@"<th rowspan='2'>Type of Document</th>

                           <th rowspan='2'align='center'>Doc. No.</th>");

            }

            strHTMLBuilder.Append(@"

            </tr>

            <tr>

                <th align='center'>NAME AND ADRESS OF THE BANK </th>

                <th align='center'>DD NO</th>

                <th align='center'>DD DATE</th>

                <th align='center'>DD</th>

                <th align='center'>CASH</th>

                <th align='center'>K2</th>

            </tr>

        </thead>

        <tbody>");

            int i = 0;
            var EmptyString = string.Empty;
            int SumDD = 0;
            int SumK2 = 0;
            int SumCash = 0;
            foreach (DataRow myRow in dt.Rows)
            {
                strHTMLBuilder.Append("<tr >");
                i++;
                foreach (DataColumn myColumn in dt.Columns)
                {
                    if (myColumn.ColumnName == "DDAmount")
                    {
                        SumDD = SumDD + Convert.ToInt32(myRow[myColumn.ColumnName]);
                    }
                    if (myColumn.ColumnName == "CashAmount")
                    {
                        SumCash = SumCash + Convert.ToInt32(myRow[myColumn.ColumnName]);
                    }
                    if (myColumn.ColumnName == "K2Amount")
                    {
                        SumK2 = SumK2 + Convert.ToInt32(myRow[myColumn.ColumnName]);
                    }

                    if (!(Select1.Items[Select1.SelectedIndex].Text.ToString() == "Duty" || Select1.Items[Select1.SelectedIndex].Text.ToString() == "Registration Fees" ||
               Select1.Items[Select1.SelectedIndex].Text.ToString() == "Cess on Stamps"))
                    {
                        if (myColumn.ColumnName == "Type_of_Doc" || myColumn.ColumnName == "Challan_Document_No") continue;
                        strHTMLBuilder.Append("<td>");
                        if (myColumn.ColumnName == "Column1")
                            strHTMLBuilder.Append(i.ToString());
                        else
                        {
                            if (myColumn.ColumnName == "Challan_creation_Date" || myColumn.ColumnName == "Challan_Remit_Date" || myColumn.ColumnName == "Date_of_Registration" || myColumn.ColumnName == "Challan_ScrollDate")
                            {
                                strHTMLBuilder.Append(Convert.ToDateTime(myRow[myColumn.ColumnName]).ToString("dd/MM/yyyy"));
                            }
                            else
                            {
                                if (myRow[myColumn.ColumnName].ToString().Trim() == string.Empty)
                                    strHTMLBuilder.Append("-");
                                else
                                    strHTMLBuilder.Append(myRow[myColumn.ColumnName].ToString());
                            }
                        }
                        strHTMLBuilder.Append("</td>");
                    }
                    else
                    {
                        strHTMLBuilder.Append("<td>");
                        if (myColumn.ColumnName == "Column1")
                            strHTMLBuilder.Append(i.ToString());
                        else
                        {
                            if (myColumn.ColumnName == "Challan_creation_Date" || myColumn.ColumnName == "Challan_Remit_Date" || myColumn.ColumnName == "Date_of_Registration" || myColumn.ColumnName == "Challan_ScrollDate")
                            {
                                strHTMLBuilder.Append(Convert.ToDateTime(myRow[myColumn.ColumnName]).ToString("dd/MM/yyyy"));
                            }
                            else
                            {
                                if (myRow[myColumn.ColumnName].ToString().Trim() == string.Empty)
                                    strHTMLBuilder.Append("-");
                                else
                                    strHTMLBuilder.Append(myRow[myColumn.ColumnName].ToString());
                            }
                        }
                        strHTMLBuilder.Append("</td>");
                    }
                }
                strHTMLBuilder.Append("</tr>");

            }
            strHTMLBuilder.Append(@"<tr>

<td Colspan='5' align='right'><b>Total :</b></td>");
            strHTMLBuilder.Append("<td>"); strHTMLBuilder.Append(SumDD); strHTMLBuilder.Append(@"</td>");
            strHTMLBuilder.Append("<td>"); strHTMLBuilder.Append(SumCash); strHTMLBuilder.Append(@"</td>");
            strHTMLBuilder.Append("<td>"); strHTMLBuilder.Append(SumK2); strHTMLBuilder.Append(@"</td></tr>");

            strHTMLBuilder.Append(@"<tr>

<td Colspan='5' align='right'><b>Grand Total :</b></td>");
            strHTMLBuilder.Append("<td Colspan='12' align='left' >"); strHTMLBuilder.Append(SumDD + SumCash + SumK2);strHTMLBuilder.Append(@"</td></tr>");
            strHTMLBuilder.Append("</tbody>");
            strHTMLBuilder.Append("</table>");
            strHTMLBuilder.Append("</body>");
            strHTMLBuilder.Append("</html>");
            string Htmltext = strHTMLBuilder.ToString();
            File.WriteAllText(Server.MapPath("~/Files/") + "Annexure.htm", Htmltext);
            Process.Start(Server.MapPath("~/Files/") + "Annexure.htm");
        }

        protected void Logout_Click(object sender, EventArgs e)
        {
            BindMainRepeater();
            CreateExcelFile(dt);
            //FormsAuthentication.SignOut();
            //Response.Redirect("/Login.aspx");
        }
        public void CreateExcelFile(DataTable Excel)
        {
             Excel = dt;
            //Clears all content output from the buffer stream.  
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=Annexure.xls");

            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            //sets font
            HttpContext.Current.Response.Write("<font style='font-size:10.0pt; font-family:Calibri;'>");
            HttpContext.Current.Response.Write("<BR><BR><BR>");           
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            HttpContext.Current.Response.Write("<Table border='1' bgColor='#ffffff' " +
              "borderColor='#000000' cellSpacing='0' cellPadding='0' " +
              "style='font-size:10.0pt; font-family:Calibri; background:white;'> <TR>");

            HttpContext.Current.Response.Write(@"<table border='1' cellspacing='0' cellpadding='0' align='center' style='text-align:center;font-size:12'>

        <thead>
            <tr>
                <th rowspan='2' align='center'>Sl No.</th>

                <th rowspan='2' align='center'>CHALLAN CREATED DATE</th>

                <th colspan='3'align='center'>DD/PAY-ORDER DETAILS</th>

                <th colspan='3'align='center'>AMOUNT REMITTED</th>

                <th rowspan='2'align='center'>K2 CHALLAN RF NO</th>

                <th rowspan='2'>DAT OF REMITTANCE TO BANK</th>

                <th rowspan='2'>KTC-25 serial NO</th>

                <th rowspan='2'>KTC -25 CHALLAN NO</th>

                <th rowspan='2'>DATE OF REALISATION (scroll date)</th>

                <th rowspan='2'>REALISATION DAYS</th>

                <th rowspan='2'>DATE OF REGISTRATION </th>");

            if (Select1.Items[Select1.SelectedIndex].Text.ToString().Contains("Duty") || Select1.Items[Select1.SelectedIndex].Text.ToString().Contains("Registr") ||

             Select1.Items[Select1.SelectedIndex].Text.ToString().Contains("Cess"))

            {

                HttpContext.Current.Response.Write(@"<th rowspan='2'>Type of Document</th>

                           <th rowspan='2'align='center'>Doc. No.</th>");

            }

            HttpContext.Current.Response.Write(@"

            </tr>

            <tr>

                <th align='center'>NAME AND ADRESS OF THE BANK </th>

                <th align='center'>DD NO</th>

                <th align='center'>DD DATE</th>

                <th align='center'>DD</th>

                <th align='center'>CASH</th>

                <th align='center'>K2</th>

            </tr>

        </thead>

        <tbody>");


            string space = "";
            int SumDD = 0;
            int SumK2 = 0;
            int SumCash = 0;
           
            int countcolumn;
            int i = 0;
            foreach (DataRow dr in Excel.Rows)
            {
                HttpContext.Current.Response.Write("<TR>");
                space = "";
                i++;
                for (countcolumn = 0; countcolumn < Excel.Columns.Count; countcolumn++)
                {
                    if (countcolumn == 5)
                    {
                        SumDD = SumDD + Convert.ToInt32(dr[countcolumn]);
                    }
                    if (countcolumn == 6)
                    {
                        SumCash = SumCash + Convert.ToInt32(dr[countcolumn]);
                    }
                    if (countcolumn == 7)
                    {
                        SumK2 = SumK2 + Convert.ToInt32(dr[countcolumn]);
                    }
                    if (!(Select1.Items[Select1.SelectedIndex].Text.ToString() == "Duty" || Select1.Items[Select1.SelectedIndex].Text.ToString() == "Registration Fees" ||
                 Select1.Items[Select1.SelectedIndex].Text.ToString() == "Cess on Stamps"))
                    {
                        if (countcolumn == 16 || countcolumn == 15) continue;
                    }                     

                   HttpContext.Current.Response.Write("<Td>");
                    if (countcolumn == 0)
                    {
                        Response.Write(space + i.ToString());
                    }
                    else
                        Response.Write(space + dr[countcolumn].ToString());
                    space = "\t";
                    HttpContext.Current.Response.Write("</Td>");
                }
                Response.Write("\n");
                HttpContext.Current.Response.Write("</TR>");
            }
            HttpContext.Current.Response.Write(@"<tr>

<td Colspan='5' align='right'><b>Total :</b></td>");
            HttpContext.Current.Response.Write("<td>"); HttpContext.Current.Response.Write(SumDD); HttpContext.Current.Response.Write(@"</td>");
            HttpContext.Current.Response.Write("<td>"); HttpContext.Current.Response.Write(SumCash); HttpContext.Current.Response.Write(@"</td>");
            HttpContext.Current.Response.Write("<td>"); HttpContext.Current.Response.Write(SumK2); HttpContext.Current.Response.Write(@"</td></tr>");
            HttpContext.Current.Response.Write("</Table>");
            HttpContext.Current.Response.Write("</font>");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
            Response.End();
        }
    }
}