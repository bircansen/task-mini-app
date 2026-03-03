import { Formik } from "formik";
import * as Yup from "yup";

const schema = Yup.object({
  title: Yup.string().trim().required("Title zorunlu.").max(120, "Max 120 karakter."),
  description: Yup.string().nullable().max(1000, "Max 1000 karakter."),
  status: Yup.mixed().oneOf(["todo", "doing", "done"], "Status geçersiz.").required(),
  assigneeUserId: Yup.number().nullable().typeError("Assignee geçersiz."),
  dueDate: Yup.string()
    .nullable()
    .matches(/^\d{4}-\d{2}-\d{2}$/, { message: "Tarih formatı geçersiz.", excludeEmptyString: true }),
});

export default function TaskModal({ open, onClose, onSave, users, initial }) {
  if (!open) return null;

  const initialValues = {
    title: initial?.title ?? "",
    description: initial?.description ?? "",
    status: initial?.status ?? "todo",
    assigneeUserId: initial?.assigneeUserId ?? "",
    dueDate: initial?.dueDate ? String(initial.dueDate).slice(0, 10) : "",
  };

  return (
    <div className="backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3 className="modal-title">{initial ? "Edit Task" : "Add Task"}</h3>
          <button className="modal-close" onClick={onClose} aria-label="Close">
            ✕
          </button>
        </div>

        <Formik
          initialValues={initialValues}
          validationSchema={schema}
          enableReinitialize
          onSubmit={(values) => {
            onSave({
              title: values.title.trim(),
              description: values.description?.trim() || null,
              status: values.status,
              assigneeUserId: values.assigneeUserId ? Number(values.assigneeUserId) : null,
              dueDate: values.dueDate || null,
            });
          }}
        >
          {({ values, errors, touched, handleChange, handleBlur, handleSubmit, isSubmitting }) => (
            <form className="modal-body" onSubmit={handleSubmit}>
              <div className="field">
                <label className="label">Title *</label>
                <input
                  className="input input--md"
                  name="title"
                  value={values.title}
                  onChange={handleChange}
                  onBlur={handleBlur}
                />
                {touched.title && errors.title && <div className="field-error">{errors.title}</div>}
              </div>

              <div className="field mt-12">
                <label className="label">Description</label>
                <textarea
                  className="textarea"
                  name="description"
                  rows={4}
                  value={values.description}
                  onChange={handleChange}
                  onBlur={handleBlur}
                />
                {touched.description && errors.description && <div className="field-error">{errors.description}</div>}
              </div>

              <div className="modal-row">
                <div className="field">
                  <label className="label">Status</label>
                  <select
                    className="select input--sm"
                    name="status"
                    value={values.status}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  >
                    <option value="todo">todo</option>
                    <option value="doing">doing</option>
                    <option value="done">done</option>
                  </select>
                  {touched.status && errors.status && <div className="field-error">{errors.status}</div>}
                </div>

                <div className="field">
                  <label className="label">Assignee</label>
                  <select
                    className="select input--md"
                    name="assigneeUserId"
                    value={values.assigneeUserId}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  >
                    <option value="">-</option>
                    {users.map((u) => (
                      <option key={u.id} value={u.id}>
                        {u.fullName || u.full_name || u.name}
                      </option>
                    ))}
                  </select>
                  {touched.assigneeUserId && errors.assigneeUserId && (
                    <div className="field-error">{errors.assigneeUserId}</div>
                  )}
                </div>

                <div className="field">
                  <label className="label">Due date</label>
                  <input
                    className="input input--sm"
                    type="date"
                    name="dueDate"
                    value={values.dueDate}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                  {touched.dueDate && errors.dueDate && <div className="field-error">{errors.dueDate}</div>}
                </div>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn btn--ghost" onClick={onClose}>
                  Cancel
                </button>
                <button type="submit" className="btn btn--primary" disabled={isSubmitting}>
                  Save
                </button>
              </div>
            </form>
          )}
        </Formik>
      </div>
    </div>
  );
}