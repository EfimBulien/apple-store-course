using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Web.Helpers;

public class PgUserConnectionInterceptor(ICurrentUserService currentUser) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetCurrentUser(connection);
        base.ConnectionOpened(connection, eventData);
    }

    private void SetCurrentUser(DbConnection connection)
    {
        if (currentUser.UserId == null) return;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT set_config('app.current_user_id', @p_user_id, true)";
        var param = cmd.CreateParameter();
        param.ParameterName = "@p_user_id";
        param.Value = currentUser.UserId.Value.ToString();
        cmd.Parameters.Add(param);
        cmd.ExecuteNonQuery();
    }
}