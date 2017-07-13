ldifde -i -f "checkin_admin.ldf" -c "CN=Schema,CN=Configuration,CN=X" "#schemaNamingContext" -k -s localhost:389 -v
pause