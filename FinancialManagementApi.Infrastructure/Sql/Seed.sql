USE FinancialManagementDb
INSERT INTO Customers (Code, Name, Email, Phone, Address)
VALUES
('CUST-001', 'Acme Trading', 'info@acme.com', '111111', 'Istanbul'),
('CUST-002', 'Blue Star Ltd', 'contact@bluestar.com', '222222', 'Ankara');

INSERT INTO Products (Code, Name, UnitPrice)
VALUES
('PRD-001', 'Office Chair', 150.00),
('PRD-002', 'Desk Lamp', 45.00),
('PRD-003', 'Monitor 24 Inch', 220.00);