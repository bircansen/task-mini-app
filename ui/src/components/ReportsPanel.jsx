import { useEffect, useState } from "react";
import { getOpenTasksByUser } from "../api/reports";

export default function ReportsPanel() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let alive = true;

    async function load() {
      setLoading(true);
      setError("");

      try {
        const res = await getOpenTasksByUser();

        // res beklenen: { success: true, data: [...] }
        const data = Array.isArray(res?.data) ? res.data : [];

        if (!alive) return;
        setRows(data);
      } catch (e) {
        if (!alive) return;
        setError(e?.message || "Rapor yüklenemedi");
        setRows([]);
      } finally {
        if (alive) setLoading(false);
      }
    }

    load();
    return () => {
      alive = false;
    };
  }, []);

  return (
    <section style={{ marginTop: 28 }}>
      <h2 style={{ margin: "0 0 12px" }}>Open Tasks By User</h2>

      {loading && <div className="loading">Loading...</div>}

      {error && (
        <div className="error">
          {error}
          <div className="muted" style={{ marginTop: 6 }}>
            İpucu: `VITE_API_BASE_URL` doğru mu? Örn: https://localhost:7259
          </div>
        </div>
      )}

      {!loading && !error && (
        <table className="table">
          <thead>
            <tr>
              <th>User</th>
              <th>Open Task Count</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r, i) => (
              <tr key={r.userId ?? r.user_id ?? r.email ?? i}>
                <td>{r.fullName || r.full_name || r.name || "-"}</td>
                <td>{r.openTaskCount ?? r.open_task_count ?? 0}</td>
              </tr>
            ))}

            {rows.length === 0 && (
              <tr>
                <td colSpan={2} className="muted">
                  No data.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}
    </section>
  );
}