
# Start backend
Start-Process powershell -ArgumentList "dotnet run --project BirthdayCollator.Server"

# Start frontend
Start-Process powershell -ArgumentList "npm run dev --prefix BirthdayCollator.Client"
