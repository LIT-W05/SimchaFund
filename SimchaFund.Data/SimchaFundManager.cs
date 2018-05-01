using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SimchaFund.Data
{
    public class SimchaFundManager
    {
        private string _connectionString;

        public SimchaFundManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddSimcha(Simcha simcha)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            using (var cmd = sqlConnection.CreateCommand())
            {
                sqlConnection.Open();
                cmd.CommandText = "INSERT INTO Simchas (Name, Date) VALUES (@name, @date) SELECT SCOPE_IDENTITY()";
                cmd.Parameters.AddWithValue("@name", simcha.Name);
                cmd.Parameters.AddWithValue("@date", simcha.Date);
                simcha.Id = (int)(decimal)cmd.ExecuteScalar();
            }
        }

        public IEnumerable<Simcha> GetAllSimchas()
        {
            var simchas = new List<Simcha>();
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Simchas";
                connection.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var simcha = new Simcha();
                    simcha.Id = (int)reader["Id"];
                    simcha.Date = (DateTime)reader["Date"];
                    simcha.Name = (string)reader["Name"];
                    SetSimchaTotals(simcha);
                    simchas.Add(simcha);
                }
            }

            return simchas;
        }

        public Simcha GetSimchaById(int simchaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Simchas WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", simchaId);
                connection.Open();
                var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                var simcha = new Simcha();
                simcha.Id = (int)reader["Id"];
                simcha.Date = (DateTime)reader["Date"];
                simcha.Name = (string)reader["Name"];
                SetSimchaTotals(simcha);
                return simcha;
            }
        }

        public int GetContributorCount()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Contributors";
                connection.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void AddContributor(Contributor contributor)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            using (var cmd = sqlConnection.CreateCommand())
            {
                sqlConnection.Open();
                cmd.CommandText = @"INSERT INTO Contributors (FirstName, LastName, CellNumber, Date, AlwaysInclude) 
                                    VALUES (@firstName, @lastName, @cellNumber, @date, @alwaysInclude); SELECT SCOPE_IDENTITY()";
                cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
                cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
                cmd.Parameters.AddWithValue("@cellNumber", contributor.CellNumber);
                cmd.Parameters.AddWithValue("@date", contributor.Date);
                cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
                contributor.Id = (int)(decimal)cmd.ExecuteScalar();

            }
        }

        public void UpdateContributor(Contributor contributor)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            using (var cmd = sqlConnection.CreateCommand())
            {
                sqlConnection.Open();
                cmd.CommandText = @"UPDATE Contributors SET FirstName = @firstName, LastName = @lastName, CellNumber = @cellNumber,
                                    Date = @date, AlwaysInclude = @alwaysInclude WHERE Id = @id";
                cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
                cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
                cmd.Parameters.AddWithValue("@cellNumber", contributor.CellNumber);
                cmd.Parameters.AddWithValue("@date", contributor.Date);
                cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
                cmd.Parameters.AddWithValue("@id", contributor.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<Contributor> GetContributors()
        {
            var contributors = new List<Contributor>();
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                connection.Open();
                cmd.CommandText = "SELECT * FROM Contributors";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var contributor = new Contributor();
                    contributor.Id = (int)reader["Id"];
                    contributor.FirstName = (string)reader["FirstName"];
                    contributor.LastName = (string)reader["LastName"];
                    contributor.CellNumber = (string)reader["CellNumber"];
                    contributor.Date = (DateTime)reader["Date"];
                    contributor.AlwaysInclude = (bool)reader["AlwaysInclude"];
                    contributor.Balance = GetContributorBalance(contributor.Id);
                    contributors.Add(contributor);
                }
            }

            return contributors;
        }

        public void AddDeposit(Deposit deposit)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Deposits (Date, Amount, ContributorId)
                                     VALUES (@date, @amount, @contributorId)";
                cmd.Parameters.AddWithValue("@date", deposit.Date);
                cmd.Parameters.AddWithValue("@amount", deposit.Amount);
                cmd.Parameters.AddWithValue("@contributorId", deposit.ContributorId);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<SimchaContributor> GetSimchaContributors(int simchaId)
        {
            var simchaContributors = new List<SimchaContributor>();
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT c.Id, c.FirstName, c.LastName, c.AlwaysInclude,
                                    cb.Amount from Contributors c
                                    LEFT JOIN Contributions cb
                                    ON c.Id = cb.ContributorId
                                    WHERE cb.SimchaId = @simchaId OR cb.SimchaId is NULL";
                cmd.Parameters.AddWithValue("@simchaId", simchaId);
                connection.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SimchaContributor sc = new SimchaContributor();
                    sc.FirstName = (string)reader["FirstName"];
                    sc.LastName = (string)reader["LastName"];
                    sc.ContributorId = (int)reader["Id"];
                    object amount = reader["Amount"];
                    if (amount != DBNull.Value)
                    {
                        sc.Amount = (decimal)amount;
                    }
                    sc.AlwaysInclude = (bool)reader["AlwaysInclude"];
                    sc.Balance = GetContributorBalance(sc.ContributorId);
                    simchaContributors.Add(sc);
                }

                return simchaContributors;
            }
        }

        public IEnumerable<SimchaContributor> GetSimchaContributorsEasy(int simchaId)
        {
            IEnumerable<Contributor> contributors = GetContributors();
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Contributions WHERE SimchaId = @simchaId";
                cmd.Parameters.AddWithValue("@simchaId", simchaId);
                connection.Open();
                var reader = cmd.ExecuteReader();
                List<Contribution> contributions = new List<Contribution>();
                while (reader.Read())
                {
                    Contribution contribution = new Contribution
                    {
                        Amount = (decimal)reader["Amount"],
                        SimchaId = simchaId,
                        ContributorId = (int)reader["ContributorId"]
                    };
                    contributions.Add(contribution);
                }

                List<SimchaContributor> result = new List<SimchaContributor>();
                foreach (Contributor contributor in contributors)
                {
                    SimchaContributor sc = new SimchaContributor();
                    sc.FirstName = contributor.FirstName;
                    sc.LastName = contributor.LastName;
                    sc.AlwaysInclude = contributor.AlwaysInclude;
                    sc.ContributorId = contributor.Id;
                    sc.Balance = contributor.Balance;
                    Contribution contribution = contributions.FirstOrDefault(c => c.ContributorId == contributor.Id);
                    if (contribution != null)
                    {
                        sc.Amount = contribution.Amount;
                    }
                    result.Add(sc);
                }

                return result;
            }

        }

        public void UpdateSimchaContributions(int simchaId, IEnumerable<ContributionInclusion> contributors)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Contributions WHERE SimchaId = @simchaId";
                cmd.Parameters.AddWithValue("@simchaId", simchaId);

                connection.Open();
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                cmd.CommandText = @"INSERT INTO Contributions (SimchaId, ContributorId, Amount)
                                    VALUES (@simchaId, @contributorId, @amount)";
                foreach (ContributionInclusion contributor in contributors.Where(c => c.Include))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@simchaId", simchaId);
                    cmd.Parameters.AddWithValue("@contributorId", contributor.ContributorId);
                    cmd.Parameters.AddWithValue("@amount", contributor.Amount);
                    cmd.ExecuteNonQuery();
                }

            }
        }

        public IEnumerable<Deposit> GetDepositsById(int contribId)
        {
            List<Deposit> deposits = new List<Deposit>();
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Deposits WHERE ContributorId = @contribId";
                cmd.Parameters.AddWithValue("@contribId", contribId);
                connection.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Deposit deposit = new Deposit();
                    deposit.Id = (int)reader["Id"];
                    deposit.Amount = (decimal)reader["Amount"];
                    deposit.Date = (DateTime)reader["Date"];
                    deposits.Add(deposit);
                }
            }

            return deposits;
        }

        public IEnumerable<Contribution> GetContributionsById(int contribId)
        {
            List<Contribution> contributions = new List<Contribution>();
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT c.*, s.Name, s.Date FROM Contributions c 
                                    JOIN Simchas s ON c.SimchaId = s.Id
                                    WHERE c.ContributorId = @contribId";
                cmd.Parameters.AddWithValue("@contribId", contribId);
                connection.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var contribution = new Contribution();
                    contribution.ContributorId = (int)reader["ContributorId"];
                    contribution.Amount = (decimal)reader["Amount"];
                    contribution.SimchaId = (int)reader["SimchaId"];
                    contribution.SimchaName = (string)reader["Name"];
                    contribution.Date = (DateTime)reader["Date"];
                    contributions.Add(contribution);
                }
            }

            return contributions;
        }

        private void SetSimchaTotals(Simcha simcha)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT ISNull(SUM(Amount), 0) as Total, Count(*) as ContributorAmount
                FROM Contributions
                WHERE SimchaId = @simchaId";
                cmd.Parameters.AddWithValue("@simchaId", simcha.Id);
                connection.Open();
                var reader = cmd.ExecuteReader();
                reader.Read();
                simcha.ContributorAmount = (int)reader["ContributorAmount"];
                simcha.Total = (decimal)reader["Total"];
            }
        }

        private decimal GetContributorBalance(int contributorId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                connection.Open();
                cmd.CommandText = "SELECT SUM(Amount) FROM Deposits WHERE ContributorId = @contribId";
                cmd.Parameters.AddWithValue("@contribId", contributorId);

                object value = cmd.ExecuteScalar();
                decimal depositTotal;
                if (value == DBNull.Value)
                {
                    depositTotal = 0;
                }

                else
                {
                    depositTotal = (decimal)value;
                }

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT IsNull(SUM(Amount),0) FROM Contributions WHERE ContributorId = @contribId";
                cmd.Parameters.AddWithValue("@contribId", contributorId);
                decimal contibutionsTotal = (decimal)cmd.ExecuteScalar();
                return depositTotal - contibutionsTotal;
            }
        }
    }
}