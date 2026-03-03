# 📝 Task Mini App

Görev takip uygulaması.

**Database:** MySQL  
**Backend:** .NET Core Web API  
**Frontend:** React (Vite)

---

## 📦 Kurulum

### 🗄 1. Database Kurulumu (MySQL)

#### Gereksinimler
- MySQL 8+
- MySQL Workbench (opsiyonel)

#### Adımlar

Proje dizinindeki `db/db.sql` dosyasını çalıştırın.

Bu işlem:
- users, tasks, task_logs tablolarını oluşturur
- Seed data (3 kullanıcı + 10 görev) ekler

---

### ⚙ 2. API Kurulumu (.NET)

#### Gereksinimler
- .NET 8 SDK
- Visual Studio veya VS Code

API klasörüne gidin:

```bash
cd api/task-mini-app
```

`appsettings.json` dosyasını düzenleyin (bkz. Configuration bölümü).

API'yi çalıştırın:

```bash
dotnet restore
dotnet run
```

#### 🔎 Swagger

API çalıştırıldığında konsolda görüntülenen base URL’nin sonuna `/swagger` ekleyerek Swagger arayüzüne erişebilirsiniz.

Örnek:

```text
Now listening on: https://localhost:{PORT}
```

Swagger adresi:

```text
https://localhost:{PORT}/swagger
```

---

### 💻 3. UI Kurulumu (React)

#### Gereksinimler
- Node.js

UI klasörüne gidin:

```bash
cd ui
```

Bağımlılıkları yükleyin:

```bash
npm install
```

Uygulamayı başlatın:

```bash
npm run dev
```

---

## 🔧 Configuration

### API – appsettings.json

`api/task-mini-app/appsettings.json` dosyasını aşağıdaki gibi düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=task_mini_app;user=root;password=YOUR_PASSWORD;"
  }
}
```

`YOUR_PASSWORD` değerini kendi MySQL şifreniz ile değiştirin.

---

### UI – .env

`ui` klasörü içinde `.env` dosyası oluşturun:

```env
VITE_API_BASE_URL=http://localhost:{PORT}
```

`{PORT}` API başlatıldığında terminalde görüntülenen port numarası olmalıdır.

---

## ⚠ Bilinen Eksikler

- Task delete endpoint eklenmedi (proje gereksinimlerinde yer almıyordu).
- Unit test eklenmedi.
- Pagination uygulanmadı.
