﻿using Vismai.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;

namespace Vismai
{
    public partial class UnmatchedData : System.Web.UI.Page
    {
        string DeptID;
        DataTable dt = new DataTable();
        string DeptName, District, DDLCode;
        string ChallanRefNumber;
        SqlConnection con;
        protected void Page_Load(object sender, EventArgs e)
        {
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
                str.Append(@"select 1, A.Challan_creation_Date,A.BankName,A.Challan_Amount,A.Challan_Ref,A.DemandDraftNumber,A.Challan_Remit_Date , B.ChallanId, B.Challan_number, B.Challan_ScrollDate,+
                          +  '' As 'Realsation Days', A.Date_of_Registration, A.Type_of_Doc, A.Challan_Document_No  from Challan A  LEFT Outer Join ImportTreasury B on A.Challan_Ref = B.Challan_RefNo  where B.Challan_ScrollDate is NULL AND B.ChallanId is NULL AND  A.Head_of_Account = '" + Select1.Items[Select1.SelectedIndex].Value.ToString() + "'  AND FORMAT(CAST(A.Date_of_Registration as datetime2),'YYYY-MM-DD')  between FORMAT(CAST('" + StartDate + "' as Datetime2),'YYYY-MM-DD') AND FORMAT( CAST('" + EndDate + "' as Datetime2) ,'YYYY-MM-DD') order by A.Date_of_Registration");
                //if (Select1.Items[Select1.SelectedIndex].Text.ToString() == "Duty")
                //    str.Append("AND B.Head_of_Account in ('0030~02~103~0~01~000','0030~02~102~0~01~000')");
                //else
                //    str.Append("AND B.Head_of_Account = '" + Select1.Items[Select1.SelectedIndex].Value.ToString().Replace('-', '~') + "'");
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

        protected void Logout_Click(object sender, EventArgs e)
        {
            BindMainRepeater();
            CreateExcelFile(dt);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            BindMainRepeater();
            string StartDate = String.Format("{0}", Request.Form["bdate"]);
            string EndDate = String.Format("{0}", Request.Form["edate"]);
            StartDate = Convert.ToDateTime(StartDate).ToString("dd/MM/yyyy");
            EndDate = Convert.ToDateTime(EndDate).ToString("dd/MM/yyyy");
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

            strHTMLBuilder.Append(@"</td><td></td><td></td></tr><tr><td style='font-size:12; padding-left:24px'>From Date: " + StartDate + "</td><td align='center' colspan='3'><h5 align='center'>ANNEXURE-A( " + Select1.Items[Select1.SelectedIndex].Text.ToString() + ") &nbsp;HoA: " + Select1.Items[Select1.SelectedIndex].Value.ToString() + "  </h5> </td><td align='right' style='font-size:12; padding-left:24px'>To Date: " + EndDate + "</td></tr></table>");

            strHTMLBuilder.Append("<table border='1' cellspacing='0' cellpadding='0' align='center' style='text-align:center;font-size:12'>");

            strHTMLBuilder.Append(@" <tr> 

                <th width='35mm' align='center'><b>Sl.No</b></th> 

                <th width='63mm' align='center'><b>Challan Created date</b></th> 

                <th width='220mm' align='center'><b>Name & Address of Bank</b></th> 

                <th width='45mm' align='center'><b>Amount</b></th> 

                           <th width='120mm' align='center'><b>K2 Challan RF No.</b></th>

                           <th width='45mm' align='center'><b>DD No.</b></th>

                           <th width='63mm' align='center'><b>Date of Remittance to Bank</b></th>

                           <th width='40mm' align='center'><b>KTC 25 Sr.No</b></th>

                           <th width='120mm' align='center'><b>KTC 25 Challan No.</b></th>

                           <th width='63mm' align='center'><b>Date of Realisation (Scroll Date)</b></th>

                           <th width='15mm' align='center'><b>Realsation days</b></th>

                           <th width='63mm' align='center'><b>Date of Registration</b></th>");

            if (Select1.Items[Select1.SelectedIndex].Text.ToString()=="Duty" || Select1.Items[Select1.SelectedIndex].Text.ToString()== "Registration Fees" ||
                Select1.Items[Select1.SelectedIndex].Text.ToString()== "Cess on Stamps")
            {
                strHTMLBuilder.Append(@"   <th width='250mm' ><b>Type of Document</b></th>
                           <th width='45mm' align='center'><b>Doc. No.</b></th><tr>");
            }
            else
            {
                strHTMLBuilder.Append(@"</tr>");
            }
            int i = 0;
            var EmptyString = string.Empty;
            int Total = 0;
            foreach (DataRow myRow in dt.Rows)
            {
                strHTMLBuilder.Append(" <tr>");
                i++;
                foreach (DataColumn myColumn in dt.Columns)
                {
                    if (myColumn.ColumnName == "Challan_Amount")
                    {
                        Total = Total + Convert.ToInt32(myRow[myColumn.ColumnName]);
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
                            if (myColumn.ColumnName == "Challan_creation_Date" || myColumn.ColumnName == "Challan_Remit_Date" || myColumn.ColumnName == "Date_of_Registration")
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
                            if (myColumn.ColumnName == "Challan_creation_Date" || myColumn.ColumnName == "Challan_Remit_Date" || myColumn.ColumnName == "Date_of_Registration")
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
            strHTMLBuilder.Append(@"<tr><td Colspan='3'><b>Total :</b></td>");
            strHTMLBuilder.Append("<td>"); strHTMLBuilder.Append(Total); strHTMLBuilder.Append(@"</td></tr>");           
            //Close tags.  

            strHTMLBuilder.Append("</table>");

            strHTMLBuilder.Append("</body>");

            strHTMLBuilder.Append("</html>");

            string Htmltext = strHTMLBuilder.ToString();

            File.WriteAllText(Server.MapPath("~/Files/") + "Unmatched.htm", Htmltext);

            Process.Start(Server.MapPath("~/Files/") + "Unmatched.htm");
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
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=Unmatched.xls");

            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            //sets font
            HttpContext.Current.Response.Write("<font style='font-size:10.0pt; font-family:Calibri;'>");
            HttpContext.Current.Response.Write("<BR><BR><BR>");
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            HttpContext.Current.Response.Write("<Table border='1' bgColor='#ffffff' " +
              "borderColor='#000000' cellSpacing='0' cellPadding='0' " +
              "style='font-size:10.0pt; font-family:Calibri; background:white;'> <TR>");
            string space = "";
            int Total = 0;
            foreach (DataColumn dcolumn in Excel.Columns)
            {
                if (!(Select1.Items[Select1.SelectedIndex].Text.ToString() == "Duty" || Select1.Items[Select1.SelectedIndex].Text.ToString() == "Registration Fees" ||
                     Select1.Items[Select1.SelectedIndex].Text.ToString() == "Cess on Stamps"))
                {
                    if (dcolumn.ColumnName == "Type_of_Doc" || dcolumn.ColumnName == "Challan_Document_No") continue;
                }
                HttpContext.Current.Response.Write("<Td>");
                HttpContext.Current.Response.Write("<B>");
                if (dcolumn.ColumnName == "Column1")
                    Response.Write(space + "Sl No.");
                else
                {
                   
                    Response.Write(space + dcolumn.ColumnName);
                    space = "\t";
                }
                HttpContext.Current.Response.Write("</B>");
                HttpContext.Current.Response.Write("</Td>");
            }
            Response.Write("\n");
            HttpContext.Current.Response.Write("</TR>");
            int countcolumn;
            int i = 0;
            foreach (DataRow dr in Excel.Rows)
            {
                HttpContext.Current.Response.Write("<TR>");
                space = "";
                i++;
                for (countcolumn = 0; countcolumn < Excel.Columns.Count; countcolumn++)
                {
                    if (countcolumn == 3)
                    {
                        Total = Total + Convert.ToInt32(dr[countcolumn]);
                    }
                    if (!(Select1.Items[Select1.SelectedIndex].Text.ToString() == "Duty" || Select1.Items[Select1.SelectedIndex].Text.ToString() == "Registration Fees" ||
               Select1.Items[Select1.SelectedIndex].Text.ToString() == "Cess on Stamps"))
                    {
                        if (countcolumn == 12 || countcolumn == 13) continue;
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
            HttpContext.Current.Response.Write(@"<tr><td Colspan='3' align='right'><b>Total :</b></td>");
            HttpContext.Current.Response.Write("<td>"); HttpContext.Current.Response.Write(Total); HttpContext.Current.Response.Write(@"</td></tr>");
            HttpContext.Current.Response.Write("</Table>");
            HttpContext.Current.Response.Write("</font>");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
            Response.End();
        }
    }
}