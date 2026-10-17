-- Migration: Seed electronics shop products
-- Table: dbo.Products (ProductMap.cs)
--
-- Inserts a catalog of electronics products across all shop categories.
-- CategoryId and BrandId are resolved by name from dbo.Categories and dbo.Brands.
-- Image holds an absolute URL to a freely licensed photo on Wikimedia Commons (960px thumbnail).
--
-- Idempotent: each product is inserted only when missing.

-- ==========================================================================================
-- LAPTOPS
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890'), N'MacBook Pro 16"', N'laptop,apple,m1-max', N'Apple MacBook Pro 16-inch with M3 Max chip, 36GB RAM, 1TB SSD', 2499.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Apple_MacBook_Pro_16%22_M2Max.jpg/960px-Apple_MacBook_Pro_16%22_M2Max.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Laptops'), (SELECT Id FROM dbo.Brands WHERE Name = N'Apple'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'B2C3D4E5-F6A7-8901-BCDE-F12345678901'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'B2C3D4E5-F6A7-8901-BCDE-F12345678901'), N'Dell XPS 15', N'laptop,dell,i9', N'Dell XPS 15 with Intel Core i9, 32GB RAM, 1TB SSD, OLED display', 1899.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/d/df/Dell_XPS_15_%282017%29.png/960px-Dell_XPS_15_%282017%29.png', (SELECT Id FROM dbo.Categories WHERE Name = N'Laptops'), (SELECT Id FROM dbo.Brands WHERE Name = N'Dell'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'C3D4E5F6-A7B8-9012-CDEF-123456789012'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'C3D4E5F6-A7B8-9012-CDEF-123456789012'), N'HP Spectre x360', N'laptop,hp,convertible', N'HP Spectre x360 14 with Intel Core i7, 16GB RAM, 1TB SSD', 1599.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/5/57/Hp_Spectre_x360.jpg/960px-Hp_Spectre_x360.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Laptops'), (SELECT Id FROM dbo.Brands WHERE Name = N'HP'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'D4E5F6A7-B8C9-0123-DEF0-234567890123'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'D4E5F6A7-B8C9-0123-DEF0-234567890123'), N'Lenovo ThinkPad X1 Carbon', N'laptop,lenovo,business', N'Lenovo ThinkPad X1 Carbon Gen 11 with Intel Core i7, 16GB RAM, 512GB SSD', 1799.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/e/e9/ThinkPad_X1_Carbon_gen7_%280%29.jpg/960px-ThinkPad_X1_Carbon_gen7_%280%29.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Laptops'), (SELECT Id FROM dbo.Brands WHERE Name = N'Lenovo'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'E5F6A7B8-C9D0-1234-EF01-345678901234'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'E5F6A7B8-C9D0-1234-EF01-345678901234'), N'ASUS ROG Zephyrus G14', N'laptop,asus,gaming', N'ASUS ROG Zephyrus G14 with AMD Ryzen 9, RTX 4060, 16GB RAM', 1699.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/c/c6/ASUS_ROG_Zephyrus_2026-08-15_G14.jpg/960px-ASUS_ROG_Zephyrus_2026-08-15_G14.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Laptops'), (SELECT Id FROM dbo.Brands WHERE Name = N'ASUS'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'F6A7B8C9-D0E1-2345-F012-456789012345'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'F6A7B8C9-D0E1-2345-F012-456789012345'), N'Acer Swift X', N'laptop,acer,budget', N'Acer Swift X with Intel Core i7, 16GB RAM, 512GB SSD, RTX 3050', 1099.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/a/a2/Acer_Swift_7_20201101a.jpg/960px-Acer_Swift_7_20201101a.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Laptops'), (SELECT Id FROM dbo.Brands WHERE Name = N'Acer'));

-- ==========================================================================================
-- SMARTPHONES
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'A7B8C9D0-E1F2-3456-0123-567890123456'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'A7B8C9D0-E1F2-3456-0123-567890123456'), N'Samsung Galaxy S24 Ultra', N'smartphone,samsung,android', N'Samsung Galaxy S24 Ultra with Snapdragon 8 Gen 3, 12GB RAM, 512GB storage', 1299.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/e/e7/SAMSUNG_Galaxy_S24_Ultra.jpg/960px-SAMSUNG_Galaxy_S24_Ultra.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Smartphones'), (SELECT Id FROM dbo.Brands WHERE Name = N'Samsung'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'B8C9D0E1-F2A3-4567-1234-678901234567'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'B8C9D0E1-F2A3-4567-1234-678901234567'), N'Google Pixel 8 Pro', N'smartphone,google,android', N'Google Pixel 8 Pro with Tensor G3, 12GB RAM, 256GB storage', 999.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/2/21/Google_Pixel_8_Pro.jpg/960px-Google_Pixel_8_Pro.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Smartphones'), (SELECT Id FROM dbo.Brands WHERE Name = N'Google'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'C9D0E1F2-A3B4-5678-2345-789012345678'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'C9D0E1F2-A3B4-5678-2345-789012345678'), N'Xiaomi 14 Pro', N'smartphone,xiaomi,android', N'Xiaomi 14 Pro with Snapdragon 8 Gen 3, 16GB RAM, 512GB storage', 899.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/7/71/%E5%B0%8F%E7%B1%B314_Pro%E5%92%8C%E5%B0%8F%E7%B1%B314%EF%BC%882024%E5%B9%B42%E6%9C%8820%E6%97%A5%EF%BC%89.jpg/960px-%E5%B0%8F%E7%B1%B314_Pro%E5%92%8C%E5%B0%8F%E7%B1%B314%EF%BC%882024%E5%B9%B42%E6%9C%8820%E6%97%A5%EF%BC%89.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Smartphones'), (SELECT Id FROM dbo.Brands WHERE Name = N'Xiaomi'));

-- ==========================================================================================
-- HEADPHONES & AUDIO
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'D0E1F2A3-B4C5-6789-3456-890123456789'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'D0E1F2A3-B4C5-6789-3456-890123456789'), N'Sony WH-1000XM5', N'headphones,sony,wireless', N'Sony WH-1000XM5 wireless noise-cancelling headphones with 30hr battery', 399.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/2/21/Sony-WH-1000XM3-kabellose-Bluetooth-Noise-Cancelling-Kopfhoerer.5.jpg/960px-Sony-WH-1000XM3-kabellose-Bluetooth-Noise-Cancelling-Kopfhoerer.5.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Headphones & Audio'), (SELECT Id FROM dbo.Brands WHERE Name = N'Sony'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'E1F2A3B4-C5D6-7890-4567-901234567890'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'E1F2A3B4-C5D6-7890-4567-901234567890'), N'Bose QuietComfort Ultra', N'headphones,bose,wireless', N'Bose QuietComfort Ultra noise-cancelling headphones with immersive audio', 429.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/d/d6/Bose_QuietComfort_35_II_Wireless_Headphones.jpg/960px-Bose_QuietComfort_35_II_Wireless_Headphones.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Headphones & Audio'), (SELECT Id FROM dbo.Brands WHERE Name = N'Bose'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'F2A3B4C5-D6E7-8901-5678-012345678901'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'F2A3B4C5-D6E7-8901-5678-012345678901'), N'Sennheiser Momentum 4', N'headphones,sennheiser,wireless', N'Sennheiser Momentum 4 wireless headphones with 60hr battery', 379.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/3/32/Momentum1_0.jpg/960px-Momentum1_0.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Headphones & Audio'), (SELECT Id FROM dbo.Brands WHERE Name = N'Sennheiser'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'A3B4C5D6-E7F8-9012-6789-123456789012'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'A3B4C5D6-E7F8-9012-6789-123456789012'), N'JBL Live 770NC', N'headphones,jbl,wireless', N'JBL Live 770NC adaptive noise-cancelling headphones with 40hr battery', 149.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/9/90/JBL_Synchros_E50BT.jpg/960px-JBL_Synchros_E50BT.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Headphones & Audio'), (SELECT Id FROM dbo.Brands WHERE Name = N'JBL'));

-- ==========================================================================================
-- SMARTWATCHES & WEARABLES
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'B4C5D6E7-F8A9-0123-7890-234567890123'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'B4C5D6E7-F8A9-0123-7890-234567890123'), N'Garmin Fenix 7', N'watch,garmin,fitness', N'Garmin Fenix 7 multisport GPS watch with solar charging and 16-day battery', 699.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/00/Garmin_Fenix_6x_Pro_Solar.jpg/960px-Garmin_Fenix_6x_Pro_Solar.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Smartwatches & Wearables'), (SELECT Id FROM dbo.Brands WHERE Name = N'Garmin'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'C5D6E7F8-A9B0-1234-8901-345678901234'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'C5D6E7F8-A9B0-1234-8901-345678901234'), N'Samsung Galaxy Watch 6', N'watch,samsung,fitness', N'Samsung Galaxy Watch 6 with Wear OS, GPS, and body composition analysis', 329.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/b/b9/Samsung_Galaxy_Watch_6.jpg/960px-Samsung_Galaxy_Watch_6.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Smartwatches & Wearables'), (SELECT Id FROM dbo.Brands WHERE Name = N'Samsung'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'D6E7F8A9-B0C1-2345-9012-456789012345'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'D6E7F8A9-B0C1-2345-9012-456789012345'), N'Garmin Venu 3', N'watch,garmin,fitness', N'Garmin Venu 3 smartwatch with AMOLED display and 14-day battery', 449.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/d/d0/Garmin_Venu_3_%28a%29.JPG/960px-Garmin_Venu_3_%28a%29.JPG', (SELECT Id FROM dbo.Categories WHERE Name = N'Smartwatches & Wearables'), (SELECT Id FROM dbo.Brands WHERE Name = N'Garmin'));

-- ==========================================================================================
-- COMPUTER COMPONENTS
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'E7F8A9B0-C1D2-3456-0123-567890123456'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'E7F8A9B0-C1D2-3456-0123-567890123456'), N'Intel Core i9-14900K', N'cpu,intel,14th-gen', N'Intel Core i9-14900K 24-core processor with turbo up to 6.0 GHz', 589.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/0a/Intel_i9-14900KF_CPU.jpg/960px-Intel_i9-14900KF_CPU.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Computer Components'), (SELECT Id FROM dbo.Brands WHERE Name = N'Intel'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'F8A9B0C1-D2E3-4567-1234-678901234567'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'F8A9B0C1-D2E3-4567-1234-678901234567'), N'AMD Ryzen 9 7950X', N'cpu,amd,ryzen-9', N'AMD Ryzen 9 7950X 16-core processor with boost up to 5.7 GHz', 549.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/a/a3/AMD_Ryzen_9_7900X.jpg/960px-AMD_Ryzen_9_7900X.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Computer Components'), (SELECT Id FROM dbo.Brands WHERE Name = N'AMD'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'A9B0C1D2-E3F4-5678-2345-789012345678'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'A9B0C1D2-E3F4-5678-2345-789012345678'), N'NVIDIA GeForce RTX 4090', N'gpu,nvidia,4090', N'NVIDIA GeForce RTX 4090 24GB GDDR6X graphics card', 1599.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/2/23/NVIDIA_RTX_4090_Founders_Edition_-_Nahaufnahme_%28ZMASLO%29.png/960px-NVIDIA_RTX_4090_Founders_Edition_-_Nahaufnahme_%28ZMASLO%29.png', (SELECT Id FROM dbo.Categories WHERE Name = N'Computer Components'), (SELECT Id FROM dbo.Brands WHERE Name = N'NVIDIA'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'B0C1D2E3-F4A5-6789-3456-890123456789'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'B0C1D2E3-F4A5-6789-3456-890123456789'), N'Logitech MX Master 3S', N'mouse,logitech,wireless', N'Logitech MX Master 3S wireless ergonomic mouse with 8K DPI sensor', 99.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/9/90/Logitech_MX_Master_3S_HS01.jpg/960px-Logitech_MX_Master_3S_HS01.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Computer Components'), (SELECT Id FROM dbo.Brands WHERE Name = N'Logitech'));

-- ==========================================================================================
-- GAMING CONSOLES
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'C1D2E3F4-A5B6-7890-4567-901234567890'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'C1D2E3F4-A5B6-7890-4567-901234567890'), N'Xbox Series X', N'console,microsoft,xbox', N'Microsoft Xbox Series X 1TB gaming console with 12 TFLOPS GPU', 499.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/3/3d/Xbox_Series_X.png/960px-Xbox_Series_X.png', (SELECT Id FROM dbo.Categories WHERE Name = N'Gaming Consoles'), (SELECT Id FROM dbo.Brands WHERE Name = N'Microsoft'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'D2E3F4A5-B6C7-8901-5678-012345678901'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'D2E3F4A5-B6C7-8901-5678-012345678901'), N'Nintendo Switch OLED', N'console,nintendo,hybrid', N'Nintendo Switch OLED model with 7-inch OLED display and 64GB storage', 349.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/8/80/Nintendo_Switch_%E2%80%93_OLED-Modell%2C_Konsole_und_Dock_20230506.png/960px-Nintendo_Switch_%E2%80%93_OLED-Modell%2C_Konsole_und_Dock_20230506.png', (SELECT Id FROM dbo.Categories WHERE Name = N'Gaming Consoles'), (SELECT Id FROM dbo.Brands WHERE Name = N'Nintendo'));

-- ==========================================================================================
-- CAMERAS & PHOTOGRAPHY
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'E3F4A5B6-C7D8-9012-6789-123456789012'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'E3F4A5B6-C7D8-9012-6789-123456789012'), N'Canon EOS R6 Mark II', N'camera,canon,mirrorless', N'Canon EOS R6 Mark II full-frame mirrorless camera with 24.2MP sensor', 2499.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/f/f8/Canon_EOS_R6_Mark_II_-_by_Henry_S%C3%B6derlund_%2852546794891%29.jpg/960px-Canon_EOS_R6_Mark_II_-_by_Henry_S%C3%B6derlund_%2852546794891%29.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Cameras & Photography'), (SELECT Id FROM dbo.Brands WHERE Name = N'Canon'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'F4A5B6C7-D8E9-0123-7890-234567890123'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'F4A5B6C7-D8E9-0123-7890-234567890123'), N'Nikon Z6 III', N'camera,nikon,mirrorless', N'Nikon Z6 III full-frame mirrorless camera with 24.5MP sensor', 2499.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/d/d5/Nikon_Z6_with_Sigma_50mm_F1.4_DG_HSM_20241004160701.jpg/960px-Nikon_Z6_with_Sigma_50mm_F1.4_DG_HSM_20241004160701.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Cameras & Photography'), (SELECT Id FROM dbo.Brands WHERE Name = N'Nikon'));

-- ==========================================================================================
-- HOME ELECTRONICS
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'A5B6C7D8-E9F0-1234-8901-345678901234'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'A5B6C7D8-E9F0-1234-8901-345678901234'), N'Philips 55" OLED 4K TV', N'tv,philips,oled', N'Philips 55-inch OLED 4K Smart TV with Ambilight and Dolby Vision', 1499.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/0c/Philips_Ambilight%2C_IFA_2015.jpg/960px-Philips_Ambilight%2C_IFA_2015.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Home Electronics'), (SELECT Id FROM dbo.Brands WHERE Name = N'Philips'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'B6C7D8E9-F0A1-2345-9012-456789012345'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'B6C7D8E9-F0A1-2345-9012-456789012345'), N'LG C3 65" OLED 4K TV', N'tv,lg,oled', N'LG C3 65-inch OLED evo 4K Smart TV with AI ThinQ', 1399.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/8/81/IFA_2012_LG%EC%A0%84%EC%9E%90_OLED_TV_%282%29.jpg/960px-IFA_2012_LG%EC%A0%84%EC%9E%90_OLED_TV_%282%29.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Home Electronics'), (SELECT Id FROM dbo.Brands WHERE Name = N'LG'));

-- ==========================================================================================
-- TABLETS
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'C7D8E9F0-A1B2-3456-0123-567890123456'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'C7D8E9F0-A1B2-3456-0123-567890123456'), N'iPad Pro 12.9"', N'tablet,apple,ipad', N'Apple iPad Pro 12.9-inch with M2 chip, 128GB Wi-Fi', 1099.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/9/94/12.9_inch_iPad_Pro_with_Smart_Keyboard.jpg/960px-12.9_inch_iPad_Pro_with_Smart_Keyboard.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Tablets'), (SELECT Id FROM dbo.Brands WHERE Name = N'Apple'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'D8E9F0A1-B2C3-4567-1234-678901234567'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'D8E9F0A1-B2C3-4567-1234-678901234567'), N'Samsung Galaxy Tab S9 Ultra', N'tablet,samsung,android', N'Samsung Galaxy Tab S9 Ultra 14.6-inch with S Pen, 256GB', 1199.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/05/Samsung_Galaxy_Tab_S9.png/960px-Samsung_Galaxy_Tab_S9.png', (SELECT Id FROM dbo.Categories WHERE Name = N'Tablets'), (SELECT Id FROM dbo.Brands WHERE Name = N'Samsung'));

-- ==========================================================================================
-- ACCESSORIES
-- ==========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'E9F0A1B2-C3D4-5678-2345-789012345678'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'E9F0A1B2-C3D4-5678-2345-789012345678'), N'Anker 737 Power Bank', N'accessories,anker,power-bank', N'Anker 737 Power Bank with 140W USB-C output and 24,000mAh capacity', 149.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/e/ee/Anker_power_bank_and_cable.jpg/960px-Anker_power_bank_and_cable.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Accessories'), (SELECT Id FROM dbo.Brands WHERE Name = N'Anker'));

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = CONVERT(UNIQUEIDENTIFIER, 'F0A1B2C3-D4E5-6789-3456-890123456789'))
    INSERT INTO dbo.Products (Id, Name, Tags, Description, Price, Created, Image, CategoryId, BrandId)
    VALUES (CONVERT(UNIQUEIDENTIFIER, 'F0A1B2C3-D4E5-6789-3456-890123456789'), N'Logitech MX Keys S', N'accessories,logitech,keyboard', N'Logitech MX Keys S wireless backlit keyboard with multi-device pairing', 109.99, GETDATE(), N'https://upload.wikimedia.org/wikipedia/commons/thumb/7/78/Logitech_MX_Keys_YR0073_Wireless_Keyboard.jpg/960px-Logitech_MX_Keys_YR0073_Wireless_Keyboard.jpg', (SELECT Id FROM dbo.Categories WHERE Name = N'Accessories'), (SELECT Id FROM dbo.Brands WHERE Name = N'Logitech'));
