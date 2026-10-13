-- Security hardening: card security codes (CVV) must never be stored.
-- New payments no longer persist the value (see PaymentMap.cs); clear any that already exist.

IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.Payments SET SecurityCode = NULL WHERE SecurityCode IS NOT NULL;
END
