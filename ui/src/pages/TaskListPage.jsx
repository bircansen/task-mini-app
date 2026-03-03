import { useEffect, useMemo, useState, useCallback } from "react";
import { createTask, getTasks, patchTaskStatus, updateTask } from "../api/tasks";
import { getUsers } from "../api/users";
import TaskModal from "../components/TaskModal";

const STATUS_OPTIONS = [
  { value: "", label: "All" },
  { value: "todo", label: "Todo" },
  { value: "doing", label: "Doing" },
  { value: "done", label: "Done" },
];

export default function TaskListPage() {
  const [tasks, setTasks] = useState([]);
  const [users, setUsers] = useState([]);

  const [status, setStatus] = useState("");
  const [assignee, setAssignee] = useState("");
  const [q, setQ] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);

  const [statusMenuForId, setStatusMenuForId] = useState(null);

  useEffect(() => {
    function onDocClick() {
      setStatusMenuForId(null);
    }
    document.addEventListener("click", onDocClick);
    return () => document.removeEventListener("click", onDocClick);
  }, []);

  const [qDebounced, setQDebounced] = useState(q);
  useEffect(() => {
    const t = setTimeout(() => setQDebounced(q), 300);
    return () => clearTimeout(t);
  }, [q]);

  const loadUsers = useCallback(async () => {
    try {
      const usersPayload = await getUsers();
      setUsers(usersPayload?.data || []);
    } catch (e) {
      setError(e.message || "Bir hata oluştu");
    }
  }, []);

  const loadTasks = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const tasksPayload = await getTasks({ status, assignee, q: qDebounced });
      setTasks(tasksPayload?.data || []);
    } catch (e) {
      setError(e.message || "Bir hata oluştu");
    } finally {
      setLoading(false);
    }
  }, [status, assignee, qDebounced]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  const userMap = useMemo(() => {
    const map = new Map();
    users.forEach((u) => map.set(u.id, u));
    return map;
  }, [users]);

  function openCreate() {
    setEditingTask(null);
    setIsModalOpen(true);
  }

  function openEdit(task) {
    setEditingTask(task);
    setIsModalOpen(true);
  }

  async function handleSave(form) {
    try {
      if (editingTask) await updateTask(editingTask.id, form);
      else await createTask(form);

      setIsModalOpen(false);
      await loadTasks();
    } catch (e) {
      alert(e.message || "Kaydetme hatası");
    }
  }

  async function handleStatusChange(taskId, newStatus) {
    try {
      await patchTaskStatus(taskId, newStatus);
      await loadTasks();
    } catch (e) {
      alert(e.message || "Status güncellenemedi");
    }
  }

  return (
    <div className="container">
      <h1 className="page-title">Task Mini App</h1>

      <div className="filters">
        <div className="field">
          <label className="label">Search</label>
          <input
            className="input input--md"
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Search in title..."
          />
        </div>

        <div className="field">
          <label className="label">Status</label>
          <select className="select input--sm" value={status} onChange={(e) => setStatus(e.target.value)}>
            {STATUS_OPTIONS.map((s) => (
              <option key={s.value} value={s.value}>
                {s.label}
              </option>
            ))}
          </select>
        </div>

        <div className="field">
          <label className="label">Assignee</label>
          <select className="select input--md" value={assignee} onChange={(e) => setAssignee(e.target.value)}>
            <option value="">All</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName || u.full_name || u.name}
              </option>
            ))}
          </select>
        </div>

        <button className="btn btn--primary" onClick={openCreate}>
          + Add Task
        </button>
      </div>

      {loading && <div className="loading">Loading...</div>}
      {error && <div className="error">Error: {error}</div>}

      {!loading && !error && (
        <table className="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Assignee</th>
              <th>Due</th>
              <th></th>
            </tr>
          </thead>

          <tbody>
            {tasks.map((t) => {
              const u = t.assigneeUserId ? userMap.get(t.assigneeUserId) : null;
              const fallbackAssigneeName = u ? (u.fullName || u.full_name || u.name) : null;

              return (
                <tr key={t.id}>
                  <td>
                    <div className="task-title">{t.title}</div>
                    {t.description && <div className="task-desc">{t.description}</div>}
                  </td>

                  <td className="status-cell">
                    <div className="status-actions">
                      <span className={`badge badge--${t.status}`}>{t.status}</span>

                      <button
                        className="btn btn--sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          setStatusMenuForId((prev) => (prev === t.id ? null : t.id));
                        }}
                      >
                        Change
                      </button>
                    </div>

                    {statusMenuForId === t.id && (
                      <div className="menu" onClick={(e) => e.stopPropagation()}>
                        {["todo", "doing", "done"].map((s) => (
                          <button
                            key={s}
                            className={`menu-item ${s === t.status ? "menu-item--active" : ""}`}
                            onClick={async () => {
                              await handleStatusChange(t.id, s);
                              setStatusMenuForId(null);
                            }}
                          >
                            {s}
                          </button>
                        ))}
                      </div>
                    )}
                  </td>

                  <td>{t.assigneeFullName || fallbackAssigneeName || "-"}</td>
                  <td>{t.dueDate ? String(t.dueDate).slice(0, 10) : "-"}</td>

                  <td>
                    <button className="btn btn--sm" onClick={() => openEdit(t)}>
                      Edit
                    </button>
                  </td>
                </tr>
              );
            })}

            {tasks.length === 0 && (
              <tr>
                <td colSpan={5} className="muted">
                  No tasks found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}

      <TaskModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSave}
        users={users}
        initial={editingTask}
      />
    </div>
  );
}