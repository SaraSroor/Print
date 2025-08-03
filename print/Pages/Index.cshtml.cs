using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace print.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly string _connectionString;

        public List<string> Branches { get; set; } = new();
        public Dictionary<string, List<string>> HndsaByBranch { get; set; } = new();

        [BindProperty]
        public string SelectedBranch { get; set; } = string.Empty;
        [BindProperty]
        public string SelectedHndsa { get; set; } = string.Empty;

        [BindProperty] public string Type { get; set; } = string.Empty;
        [BindProperty] public int Quantity { get; set; }
        [BindProperty] public string Specifications { get; set; } = string.Empty;
        [BindProperty] public decimal Dispensed { get; set; }
        [BindProperty] public decimal Remaining { get; set; }

        public List<(string Branch, string Hndsa, string Type, int Quantity, string Specs, decimal Dispensed, decimal Remaining)> ReportEntries { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public void OnGet()
        {
            LoadBranchesAndHndsa();
        }

        public void OnPost()
        {
            LoadBranchesAndHndsa();
        }


        private void LoadBranchesAndHndsa()
        {
            Branches.Clear();
            HndsaByBranch.Clear();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var query = @"SELECT branch_name_union, hndsa_name_union FROM [print].dbo.[LKP_Branches_Hndsat]";

            //var query = @"SELECT DISTINCT branch_name_union, hndsa_name_union FROM dbo.LKP_Branches_Hndsat";
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var branch = reader.GetString(0);
                var hndsa = reader.GetString(1);

                if (!Branches.Contains(branch))
                    Branches.Add(branch);

                if (!HndsaByBranch.ContainsKey(branch))
                    HndsaByBranch[branch] = new List<string>();

                HndsaByBranch[branch].Add(hndsa);
            }
        }

        public IActionResult OnPostSave()
        {
            if (string.IsNullOrWhiteSpace(SelectedBranch) || string.IsNullOrWhiteSpace(SelectedHndsa) ||
                string.IsNullOrWhiteSpace(Type) || string.IsNullOrWhiteSpace(Specifications) || Quantity <= 0)
            {
                ModelState.AddModelError(string.Empty, "All fields are required and must be valid.");
                LoadBranchesAndHndsa();
                return Page();
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"INSERT INTO Entries (BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining)
                    VALUES (@branch, @hndsa, @type, @qty, @specs, @disp, @rem)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@branch", SelectedBranch);
            cmd.Parameters.AddWithValue("@hndsa", SelectedHndsa);
            cmd.Parameters.AddWithValue("@type", Type);
            cmd.Parameters.AddWithValue("@qty", Quantity);
            cmd.Parameters.AddWithValue("@specs", Specifications);
            cmd.Parameters.AddWithValue("@disp", Dispensed);
            cmd.Parameters.AddWithValue("@rem", Remaining);
            cmd.ExecuteNonQuery();

            return RedirectToPage();
        }

        //public IActionResult OnPostShowReport()
        //{
        //    ReportEntries.Clear();

        //    using var conn = new SqlConnection(_connectionString);
        //    conn.Open();
        //    var sql = @"SELECT BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining
        //            FROM Entries WHERE BranchCodeUnion = @branch";
        //    using var cmd = new SqlCommand(sql, conn);
        //    cmd.Parameters.AddWithValue("@branch", SelectedBranch);
        //    using var reader = cmd.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        ReportEntries.Add((
        //            reader.GetString(0),
        //            reader.GetString(1),
        //            reader.GetString(2),
        //            reader.GetInt32(3),
        //            reader.GetString(4),
        //            reader.GetDecimal(5),
        //            reader.GetDecimal(6)
        //        ));
        //    }

        //    LoadBranchesAndHndsa();
        //    return Page();
        //}


        public IActionResult OnPostShowReport()
        {
            ReportEntries.Clear();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            string sql;
            SqlCommand cmd;

            if (!string.IsNullOrWhiteSpace(SelectedBranch) && !string.IsNullOrWhiteSpace(SelectedHndsa))
            {
                // Both branch and ????? selected
                sql = @"SELECT BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining
                FROM Entries
                WHERE BranchCodeUnion = @branch AND UnionCode = @hndsa";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@branch", SelectedBranch);
                cmd.Parameters.AddWithValue("@hndsa", SelectedHndsa);
            }
            else if (!string.IsNullOrWhiteSpace(SelectedBranch))
            {
                // Only branch selected
                sql = @"SELECT BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining
                FROM Entries
                WHERE BranchCodeUnion = @branch";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@branch", SelectedBranch);
            }
            else
            {
                // No filter – show all
                sql = @"SELECT BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining
                FROM Entries";
                cmd = new SqlCommand(sql, conn);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ReportEntries.Add((
                    reader.GetString(0),   // Branch
                    reader.GetString(1),   // ?????
                    reader.GetString(2),   // Type
                    reader.GetInt32(3),    // Quantity
                    reader.GetString(4),   // Specifications
                    reader.GetDecimal(5),  // Dispensed
                    reader.GetDecimal(6)   // Remaining


                ));
            }

            LoadBranchesAndHndsa(); // Keep dropdowns alive
            return Page();
        }




    }
}









//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Configuration;
////using Microsoft.Data.SqlClient;

//using System.Collections.Generic;
//using System.Data.SqlClient;

//namespace print.Pages
//{
//    public class IndexModel : PageModel
//    {
//        private readonly ILogger<IndexModel> _logger;
//        private readonly string _connectionString;

//        public List<string> Sectors { get; set; } = new List<string>();
//        public Dictionary<string, List<string>> BranchesBySector { get; set; } = new Dictionary<string, List<string>>();


//        [BindProperty]
//        public string SelectedSector { get; set; } = string.Empty;

//        [BindProperty]
//        public string SelectedBranch { get; set; } = string.Empty;

//        [BindProperty]
//        public string Type { get; set; } = string.Empty;
//        [BindProperty]
//        public int Quantity { get; set; } 
//        [BindProperty]
//        public string Specifications { get; set; } = string.Empty;
//        [BindProperty]
//        public decimal Dispensed { get; set; }
//        [BindProperty]
//        public decimal Remaining { get; set; }

//        public List<(string Branch, string Sector, string Type, int Quantity, string Specs, decimal Dispensed, decimal Remaining)> ReportEntries { get; set; } = new();
//        public List<string> BranchList { get; set; } = new();


//        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
//        {
//            _logger = logger;
//            _connectionString = config.GetConnectionString("DefaultConnection");

//        }


//        public void OnGet()
//        {
//            LoadSectorsAndBranches();

//        }
//        private void LoadSectorsAndBranches()
//        {
//            using var connection = new SqlConnection(_connectionString);
//            connection.Open();
//            var query = @"SELECT branch_name_union, hndsa_name_union FROM [print].dbo.[LKP_Branches_Hndsat]";
//            using var command = new SqlCommand(query, connection);
//            using var reader = command.ExecuteReader();
//            while (reader.Read())
//            {
//                var branch = reader.GetString(0);
//                var sector = reader.GetString(1);
//                if (!BranchesBySector.ContainsKey(branch))
//                {
//                    BranchesBySector[branch] = new List<string>();
//                    BranchList.Add(branch);
//                }
//                BranchesBySector[branch].Add(sector);
//            }
//        }
//        //private void LoadSectorsAndBranches()
//        //{
//        //    using var connection = new SqlConnection(_connectionString);
//        //    connection.Open();
//        //    var query = @"SELECT branch_name_union, hndsa_name_union FROM dbo.LKP_Branches_Hndsat";
//        //    using var command = new SqlCommand(query, connection);
//        //    using var reader = command.ExecuteReader();
//        //    while (reader.Read())
//        //    {
//        //        var sector = reader["branch_name_union"].ToString();
//        //        var branch = reader["hndsa_name_union"].ToString();
//        //        if (!BranchesBySector.ContainsKey(sector))
//        //        {
//        //            BranchesBySector[sector] = new List<string>();
//        //            Sectors.Add(sector);
//        //        }
//        //        BranchesBySector[sector].Add(branch);
//        //    }
//        //}


//        //public IActionResult OnPostSave()
//        //{
//        //    using var connection = new SqlConnection(_connectionString);
//        //    connection.Open();
//        //    var insert = @"INSERT INTO Entries (BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining) 
//        //                   VALUES (@branch, @sector, @type, @qty, @specs, @disp, @rem)";
//        //    using var cmd = new SqlCommand(insert, connection);
//        //    cmd.Parameters.AddWithValue("@branch", SelectedBranch);
//        //    cmd.Parameters.AddWithValue("@sector", SelectedSector);
//        //    cmd.Parameters.AddWithValue("@type", Type);
//        //    cmd.Parameters.AddWithValue("@qty", Quantity);
//        //    cmd.Parameters.AddWithValue("@specs", Specifications);
//        //    cmd.Parameters.AddWithValue("@disp", Dispensed);
//        //    cmd.Parameters.AddWithValue("@rem", Remaining);
//        //    cmd.ExecuteNonQuery();
//        //    return RedirectToPage();
//        //}



//        public IActionResult OnPostSave()
//        {
//            using var connection = new SqlConnection(_connectionString);
//            connection.Open();
//            var insert = @"INSERT INTO Entries (BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining) 
//                           VALUES (@branch, @sector, @type, @qty, @specs, @disp, @rem)";
//            using var cmd = new SqlCommand(insert, connection);
//            cmd.Parameters.AddWithValue("@branch", SelectedBranch);
//            cmd.Parameters.AddWithValue("@sector", SelectedSector);
//            cmd.Parameters.AddWithValue("@type", Type);
//            cmd.Parameters.AddWithValue("@qty", Quantity);
//            cmd.Parameters.AddWithValue("@specs", Specifications);
//            cmd.Parameters.AddWithValue("@disp", Dispensed);
//            cmd.Parameters.AddWithValue("@rem", Remaining);
//            cmd.ExecuteNonQuery();
//            return RedirectToPage();
//        }

//        public IActionResult OnPostShowReport()
//        {
//            ReportEntries.Clear();
//            using var connection = new SqlConnection(_connectionString);
//            connection.Open();
//            var query = @"SELECT BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining FROM Entries WHERE BranchCodeUnion = @branch";
//            using var cmd = new SqlCommand(query, connection);
//            cmd.Parameters.AddWithValue("@branch", SelectedBranch);
//            using var reader = cmd.ExecuteReader();
//            while (reader.Read())
//            {
//                ReportEntries.Add(
//                    (
//                        reader.GetString(0),
//                        reader.GetString(1),
//                        reader.GetString(2),
//                        reader.GetInt32(3),
//                        reader.GetString(4),
//                        reader.GetDecimal(5),
//                        reader.GetDecimal(6)
//                    )
//                );
//            }
//            // repopulate for rendering
//            LoadSectorsAndBranches();
//            return Page();
//        }


//        //public void OnPostShowReport()
//        //{
//        //    ReportEntries = new List<(string, string, string, int, string, decimal, decimal)>();
//        //    using var connection = new SqlConnection(_connectionString);
//        //    connection.Open();
//        //    var query = @"SELECT BranchCodeUnion, UnionCode, Type, Quantity, Specifications, Dispensed, Remaining FROM Entries 
//        //                 WHERE BranchCodeUnion = @branch AND UnionCode = @sector";
//        //    using var cmd = new SqlCommand(query, connection);
//        //    cmd.Parameters.AddWithValue("@branch", SelectedBranch);
//        //    cmd.Parameters.AddWithValue("@sector", SelectedSector);
//        //    using var reader = cmd.ExecuteReader();
//        //    while (reader.Read())
//        //    {
//        //        ReportEntries.Add(
//        //            (
//        //                reader.GetString(0),
//        //                reader.GetString(1),
//        //                reader.GetString(2),
//        //                reader.GetInt32(3),
//        //                reader.GetString(4),
//        //                reader.GetDecimal(5),
//        //                reader.GetDecimal(6)
//        //            )
//        //        );
//        //    }
//        //}



//    }
//}
