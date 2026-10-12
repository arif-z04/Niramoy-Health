-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 014: SEED DEMO DATA
-- =============================================================================
-- Description:
-- Populates authentic, realistic demonstration data:
--   - Leading Bangladeshi pharmaceutical manufacturers (Square, Beximco, Incepta, etc.)
--   - Top consumer healthcare brands & generics
--   - Prescription (Rx) medicines, OTC products, medical devices, baby care
--   - Warehouses, inventory stock, and traceable medicine batches
--   - Staff and customer accounts (with PBKDF2/Argon2 placeholder password hashes)
--   - Delivery addresses, completed & processing orders, payment transactions
--   - Verified pharmacist prescription reviews
--   - Promotional coupons, banners, and verified product reviews
-- =============================================================================

BEGIN;

-- 1. Pharmaceutical Manufacturers
INSERT INTO manufacturers (slug, name, name_bn, country, license_number) VALUES
('square-pharmaceuticals', 'Square Pharmaceuticals PLC', 'স্কয়ার ফার্মাসিউটিক্যালস পিএলসি', 'Bangladesh', 'DAR-001-SQR'),
('beximco-pharmaceuticals', 'Beximco Pharmaceuticals Ltd', 'বেক্সিমকো ফার্মাসিউটিক্যালস লি.', 'Bangladesh', 'DAR-002-BEX'),
('incepta-pharmaceuticals', 'Incepta Pharmaceuticals Ltd', 'ইনসেপ্টা ফার্মাসিউটিক্যালস লি.', 'Bangladesh', 'DAR-003-INC'),
('renata-limited', 'Renata Limited', 'রেনাটা লিমিটেড', 'Bangladesh', 'DAR-004-REN'),
('acme-laboratories', 'The ACME Laboratories Ltd', 'দি একমি ল্যাবরেটরিজ লি.', 'Bangladesh', 'DAR-005-ACM'),
('aci-limited', 'ACI Limited (Healthcare)', 'এসিআই লিমিটেড', 'Bangladesh', 'DAR-006-ACI'),
('healthcare-pharmaceuticals', 'Healthcare Pharmaceuticals Ltd', 'হেলথকেয়ার ফার্মাসিউটিক্যালস লি.', 'Bangladesh', 'DAR-007-HPL'),
('eskayef-pharmaceuticals', 'Eskayef Pharmaceuticals Ltd', 'এসকেএফ ফার্মাসিউটিক্যালস লি.', 'Bangladesh', 'DAR-008-SKF')
ON CONFLICT (slug) DO NOTHING;

-- 2. Brands
INSERT INTO brands (slug, name, name_bn, description) VALUES
('square', 'Square', 'স্কয়ার', 'Bangladesh''s premier pharmaceutical and healthcare brand.'),
('beximco', 'Beximco', 'বেক্সিমকো', 'Leading FDA-approved medicine manufacturer in Bangladesh.'),
('savlon', 'Savlon', 'স্যাভলন', 'Trusted hygiene and antiseptic care by ACI.'),
('dettol', 'Dettol', 'ডেটোল', 'Worldwide leader in germ protection and family hygiene.'),
('omron', 'Omron Healthcare', 'ওমরন হেলথকেয়ার', 'Precision Japanese medical monitors and nebulizers.'),
('accu-chek', 'Accu-Chek', 'একু-চেক', 'Gold standard in blood glucose monitoring by Roche.'),
('huggies', 'Huggies', 'হাগিস', 'Premium baby diapers and wipes by Kimberly-Clark.'),
('cetaphil', 'Cetaphil', 'সেটাফিল', 'Dermatologist-recommended sensitive skincare.')
ON CONFLICT (slug) DO NOTHING;

-- 3. Core Generics (Active Ingredients)
INSERT INTO generics (slug, name, name_bn, therapeutic_class, indication, dosage_guidelines, pregnancy_category) VALUES
('paracetamol-caffeine', 'Paracetamol + Caffeine', 'প্যারাসিটামল + ক্যাফেইন', 'Analgesic & Antipyretic', 'Relief from headache, migraine, fever, toothache and body aches.', '1 to 2 tablets every 4-6 hours as needed. Maximum 8 tablets daily.', 'B'),
('omeprazole', 'Omeprazole', 'ওমিপ্রাজল', 'Proton Pump Inhibitor (PPI)', 'Treatment of gastric and duodenal ulcers, GERD, and hyperacidity.', '20mg to 40mg once daily before meals in the morning.', 'C'),
('esomeprazole', 'Esomeprazole', 'ইসোমিপ্রাজল', 'Proton Pump Inhibitor (PPI)', 'Gastroesophageal reflux disease (GERD), erosive esophagitis, acid reflux.', '20mg to 40mg once daily 30-60 minutes before breakfast.', 'B'),
('azithromycin', 'Azithromycin', 'অ্যাজিথ্রোমাইসিন', 'Macrolide Antibiotic', 'Respiratory tract infections, sinusitis, skin and soft tissue infections.', '500mg once daily for 3 to 5 days as prescribed by physician.', 'B'),
('montelukast', 'Montelukast', 'মন্টেলুকাস্ট', 'Leukotriene Receptor Antagonist', 'Prophylaxis and chronic treatment of asthma and allergic rhinitis.', '10mg once daily in the evening.', 'B'),
('fexofenadine', 'Fexofenadine', 'ফেক্সোফেনাডিন', 'Non-sedating Antihistamine', 'Seasonal allergic rhinitis, sneezing, runny nose, itchy skin.', '120mg or 180mg once daily with water.', 'C'),
('metformin-hydrochloride', 'Metformin Hydrochloride', 'মেটফরমিন হাইড্রোক্লোরাইড', 'Biguanide Antidiabetic', 'Type 2 diabetes mellitus management to control blood glucose.', '500mg or 850mg two or three times daily with or after meals.', 'B')
ON CONFLICT (slug) DO NOTHING;

-- 4. Products Master Catalog
INSERT INTO products (
    sku, slug, name, name_bn, category_id, brand_id, manufacturer_id, generic_id, dosage_form_id,
    strength, pack_size, unit_of_measure, units_per_pack, base_price, discount_price,
    is_prescription_required, is_otc, description, description_bn, is_featured, badge, avg_rating, review_count
) VALUES
-- Product 1: Napa Extra (OTC)
(
    'MED-SQR-001', 'napa-extra-tablet', 'Napa Extra 500mg/65mg', 'নাপা এক্সট্রা ৫০০ মি.গ্রা./৬৫ মি.গ্রা.',
    (SELECT id FROM categories WHERE slug = 'fever-pain-relief'),
    (SELECT id FROM brands WHERE slug = 'square'),
    (SELECT id FROM manufacturers WHERE slug = 'square-pharmaceuticals'),
    (SELECT id FROM generics WHERE slug = 'paracetamol-caffeine'),
    (SELECT id FROM dosage_forms WHERE name = 'Tablet'),
    '500 mg + 65 mg', '10 tablets per strip', 'Strip', 10,
    30.00, 27.00, FALSE, TRUE,
    'Fast-acting relief from intense fever, tension headache, migraine, toothache, and body pain.',
    'তীব্র জ্বর, মাথাব্যথা, মাইগ্রেন এবং শরীরের ব্যথায় দ্রুত ও কার্যকর উপশম।',
    TRUE, 'Best Seller', 4.85, 42
),
-- Product 2: Seclo 20mg (Rx)
(
    'MED-SQR-002', 'seclo-20mg-capsule', 'Seclo 20mg Capsule', 'সেকলো ২০ মি.গ্রা. ক্যাপসুল',
    (SELECT id FROM categories WHERE slug = 'gastric-antacids'),
    (SELECT id FROM brands WHERE slug = 'square'),
    (SELECT id FROM manufacturers WHERE slug = 'square-pharmaceuticals'),
    (SELECT id FROM generics WHERE slug = 'omeprazole'),
    (SELECT id FROM dosage_forms WHERE name = 'Capsule'),
    '20 mg', '10 capsules per strip', 'Strip', 10,
    70.00, 63.00, TRUE, FALSE,
    'Proven proton pump inhibitor for severe gastric acidity, heartburn, and peptic ulcer treatment.',
    'গ্যাস্ট্রিক আলসার, বুক জ্বালাপোড়া এবং অতিরিক্ত অ্যাসিড সমস্যার জন্য নির্ভরযোগ্য ক্যাপসুল।',
    TRUE, 'Prescription', 4.90, 88
),
-- Product 3: Maxpro 20mg (Rx)
(
    'MED-BEX-001', 'maxpro-20mg-tablet', 'Maxpro 20mg Tablet', 'ম্যাক্সপ্রো ২০ মি.গ্রা. ট্যাবলেট',
    (SELECT id FROM categories WHERE slug = 'gastric-antacids'),
    (SELECT id FROM brands WHERE slug = 'beximco'),
    (SELECT id FROM manufacturers WHERE slug = 'beximco-pharmaceuticals'),
    (SELECT id FROM generics WHERE slug = 'esomeprazole'),
    (SELECT id FROM dosage_forms WHERE name = 'Tablet'),
    '20 mg', '10 tablets per strip', 'Strip', 10,
    80.00, 72.00, TRUE, FALSE,
    'Next-generation esomeprazole for hyperacidity, GERD, and healing of erosive esophagitis.',
    'উন্নত ইসোমিপ্রাজল ফর্মুলা যা দীর্ঘস্থায়ী গ্যাস্ট্রিক ও খাদ্যনালীর প্রদাহ দূর করে।',
    TRUE, 'Top Rated', 4.80, 56
),
-- Product 4: Zimax 500mg (Rx)
(
    'MED-BEX-002', 'zimax-500mg-tablet', 'Zimax 500mg Tablet', 'জিম্যাক্স ৫০০ মি.গ্রা. ট্যাবলেট',
    (SELECT id FROM categories WHERE slug = 'antibiotics'),
    (SELECT id FROM brands WHERE slug = 'beximco'),
    (SELECT id FROM manufacturers WHERE slug = 'beximco-pharmaceuticals'),
    (SELECT id FROM generics WHERE slug = 'azithromycin'),
    (SELECT id FROM dosage_forms WHERE name = 'Tablet'),
    '500 mg', '3 tablets per box', 'Box', 3,
    135.00, 125.00, TRUE, FALSE,
    'Broad-spectrum azithromycin antibiotic indicated for respiratory tract and ENT infections.',
    'শ্বাসতন্ত্র এবং গলার সংক্রমণের চিকিৎসায় বিশ্বমানের অ্যান্টিবায়োটিক।',
    FALSE, 'Prescription', 4.75, 19
),
-- Product 5: Fexo 120mg (OTC)
(
    'MED-SQR-003', 'fexo-120mg-tablet', 'Fexo 120mg Tablet', 'ফেক্সো ১২০ মি.গ্রা. ট্যাবলেট',
    (SELECT id FROM categories WHERE slug = 'cold-cough-allergy'),
    (SELECT id FROM brands WHERE slug = 'square'),
    (SELECT id FROM manufacturers WHERE slug = 'square-pharmaceuticals'),
    (SELECT id FROM generics WHERE slug = 'fexofenadine'),
    (SELECT id FROM dosage_forms WHERE name = 'Tablet'),
    '120 mg', '10 tablets per strip', 'Strip', 10,
    100.00, 90.00, FALSE, TRUE,
    'Non-drowsy 24-hour relief from allergy symptoms, allergic rhinitis, sneezing, and skin itching.',
    'ঘুমহীন ২৪ ঘণ্টার অ্যালার্জি ও সর্দি প্রতিরোধক ওষুধ।',
    TRUE, 'Popular', 4.78, 31
),
-- Product 6: Monas 10mg (Rx)
(
    'MED-ACM-001', 'monas-10mg-tablet', 'Monas 10mg Tablet', 'মোনাস ১০ মি.গ্রা. ট্যাবলেট',
    (SELECT id FROM categories WHERE slug = 'respiratory-asthma'),
    (SELECT id FROM brands WHERE slug = 'square'),
    (SELECT id FROM manufacturers WHERE slug = 'acme-laboratories'),
    (SELECT id FROM generics WHERE slug = 'montelukast'),
    (SELECT id FROM dosage_forms WHERE name = 'Tablet'),
    '10 mg', '10 tablets per strip', 'Strip', 10,
    170.00, 153.00, TRUE, FALSE,
    'Effective leukotriene blocker for chronic bronchial asthma management and seasonal allergy prevention.',
    'ব্রঙ্কিয়াল অ্যাজমা ও দীর্ঘস্থায়ী শ্বাসকষ্ট প্রতিরোধক ট্যাবলেট।',
    FALSE, 'Prescription', 4.88, 25
),
-- Product 7: Savlon Antiseptic Liquid 500ml (OTC)
(
    'OTC-ACI-001', 'savlon-antiseptic-liquid-500ml', 'Savlon Antiseptic Liquid 500ml', 'স্যাভলন অ্যান্টিসেপটিক লিকুইড ৫০০ মি.লি.',
    (SELECT id FROM categories WHERE slug = 'first-aid-antiseptics'),
    (SELECT id FROM brands WHERE slug = 'savlon'),
    (SELECT id FROM manufacturers WHERE slug = 'aci-limited'),
    NULL, NULL,
    '500 ml', '500 ml bottle', 'Bottle', 1,
    210.00, 195.00, FALSE, TRUE,
    'Trusted hospital-grade antiseptic liquid for minor cuts, wounds, first aid, and germ protection.',
    'কাটা-ছেঁড়া, ক্ষত পরিষ্কার ও জীবাণুমুক্ত রাখার জন্য পরিবারের বিশ্বস্ত অ্যান্টিসেপটিক।',
    TRUE, 'Essential', 4.92, 110
),
-- Product 8: Omron HEM-7120 Blood Pressure Monitor
(
    'DEV-OMR-001', 'omron-hem-7120-blood-pressure-monitor', 'Omron HEM-7120 Digital BP Monitor', 'ওমরন এইচইএম-৭১২০ ডিজিটাল বিপি মনিটর',
    (SELECT id FROM categories WHERE slug = 'blood-pressure-monitors'),
    (SELECT id FROM brands WHERE slug = 'omron'),
    NULL, NULL, NULL,
    'Automatic', '1 unit with cuff and batteries', 'Piece', 1,
    3450.00, 3150.00, FALSE, TRUE,
    'Clinically validated digital upper arm blood pressure monitor with Intellisense technology and 3-year warranty.',
    'জাপানি প্রযুক্তির নির্ভুল ডিজিটাল রক্তচাপ মাপার মেশিন। ৩ বছরের ওয়ারেন্টিসহ।',
    TRUE, 'Best Seller', 4.95, 64
),
-- Product 9: Accu-Chek Instant Blood Glucose Meter
(
    'DEV-RCH-001', 'accu-chek-instant-glucometer', 'Accu-Chek Instant Glucometer Pack', 'একু-চেক ইনস্ট্যান্ট গ্লুকোমিটার প্যাক',
    (SELECT id FROM categories WHERE slug = 'glucose-meters'),
    (SELECT id FROM brands WHERE slug = 'accu-chek'),
    NULL, NULL, NULL,
    'Instant Kit', 'Meter + 10 test strips + lancing device', 'Kit', 1,
    2200.00, 1980.00, FALSE, TRUE,
    'Effortless blood sugar monitoring with target range indicator and instant visual clarity.',
    'সহজ ও নিখুঁত রক্তের শর্করা মাপার গ্লুকোমিটার কিট। তাৎক্ষণিক ফলাফল।',
    TRUE, 'Top Device', 4.90, 77
),
-- Product 10: Huggies Dry Diaper Pants Large 54 Pcs
(
    'BABY-HUG-001', 'huggies-dry-diaper-pants-large-54', 'Huggies Dry Diaper Pants (Large, 54 pcs)', 'হাগিস ড্রাই ডায়াপার প্যান্টস (লার্জ, ৫৪ পিস)',
    (SELECT id FROM categories WHERE slug = 'diapers-wipes'),
    (SELECT id FROM brands WHERE slug = 'huggies'),
    NULL, NULL, NULL,
    '9-14 kg', '54 pants pack', 'Pack', 1,
    1850.00, 1699.00, FALSE, TRUE,
    'Up to 12 hours overnight dryness with breathable bubble-bed technology for active toddlers.',
    '১২ ঘণ্টা পর্যন্ত শুষ্কতা নিশ্চিতকারী আরামদায়ক বেবি ডায়াপার প্যান্ট।',
    TRUE, 'Hot Deal', 4.82, 53
)
ON CONFLICT (sku) DO NOTHING;

-- 5. Product Secondary Categories (Mapping)
INSERT INTO product_categories (product_id, category_id, is_primary)
SELECT p.id, p.category_id, TRUE FROM products p
ON CONFLICT DO NOTHING;

-- 6. Warehouses & Fulfillment Hubs
INSERT INTO warehouses (code, name, division_id, district_id, address, contact_phone, is_central_hub) VALUES
('HUB-TEJGAON', 'Central Pharmacy Hub - Tejgaon', 
 (SELECT id FROM divisions WHERE code = 'DHAKA'), 
 (SELECT id FROM districts WHERE code = 'DHA'), 
 'Plot 14, Industrial Area, Tejgaon, Dhaka-1208', '+8801711001100', TRUE),
('HUB-CHATTOGRAM', 'Chattogram Regional Hub - Agrabad', 
 (SELECT id FROM divisions WHERE code = 'CHATTOGRAM'), 
 (SELECT id FROM districts WHERE code = 'CTG'), 
 'Commercial Area, Agrabad, Chattogram-4100', '+8801811002200', FALSE)
ON CONFLICT (code) DO NOTHING;

-- 7. Inventory Levels
INSERT INTO inventory (product_id, warehouse_id, quantity_on_hand, quantity_reserved, reorder_level, reorder_quantity)
SELECT 
    p.id,
    w.id,
    CASE 
        WHEN p.sku = 'MED-SQR-001' THEN 500
        WHEN p.sku = 'MED-SQR-002' THEN 350
        WHEN p.sku = 'MED-BEX-001' THEN 280
        WHEN p.sku = 'DEV-OMR-001' THEN 45
        ELSE 120
    END AS quantity_on_hand,
    CASE 
        WHEN p.sku = 'MED-SQR-001' THEN 10
        WHEN p.sku = 'DEV-OMR-001' THEN 2
        ELSE 0
    END AS quantity_reserved,
    20 AS reorder_level,
    100 AS reorder_quantity
FROM products p
CROSS JOIN warehouses w
WHERE w.code = 'HUB-TEJGAON'
ON CONFLICT (product_id, warehouse_id) DO NOTHING;

-- 8. Medicine Batches (Expiry validation)
INSERT INTO medicine_batches (product_id, warehouse_id, batch_number, mfg_date, expiry_date, quantity_received, quantity_remaining, unit_cost)
SELECT 
    p.id,
    w.id,
    'BAT-2026-A' || p.id,
    CURRENT_DATE - INTERVAL '60 days',
    CURRENT_DATE + INTERVAL '540 days',
    300,
    CASE WHEN p.sku = 'MED-SQR-001' THEN 290 ELSE 150 END,
    p.base_price * 0.65
FROM products p
CROSS JOIN warehouses w
WHERE w.code = 'HUB-TEJGAON' AND (p.is_prescription_required = TRUE OR p.generic_id IS NOT NULL)
ON CONFLICT (product_id, warehouse_id, batch_number) DO NOTHING;

-- 9. Demo Users (Admin, Pharmacist, Customers)
INSERT INTO users (phone_number, email, password_hash, full_name, is_phone_verified, is_email_verified, status) VALUES
('+8801700000001', 'superadmin@niramoyhealth.com.bd', '$2a$12$e80yq9gq0p5k.niramoy.admin.hash.placeholder.secure', 'Dr. Mahmudul Hasan', TRUE, TRUE, 'active'),
('+8801700000002', 'pharmacist@niramoyhealth.com.bd', '$2a$12$e80yq9gq0p5k.niramoy.pharm.hash.placeholder.secure', 'Nusrat Jahan, B.Pharm', TRUE, TRUE, 'active'),
('+8801711223344', 'tanvir.ahmed@example.com', '$2a$12$e80yq9gq0p5k.niramoy.cust1.hash.placeholder.secure', 'Tanvir Ahmed', TRUE, TRUE, 'active'),
('+8801811556677', 'sultana.razya@example.com', '$2a$12$e80yq9gq0p5k.niramoy.cust2.hash.placeholder.secure', 'Sultana Razya', TRUE, TRUE, 'active')
ON CONFLICT (phone_number) DO NOTHING;

-- Assign Roles
INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id FROM users u, roles r 
WHERE u.phone_number = '+8801700000001' AND r.name = 'super_admin'
ON CONFLICT DO NOTHING;

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id FROM users u, roles r 
WHERE u.phone_number = '+8801700000002' AND r.name = 'pharmacist'
ON CONFLICT DO NOTHING;

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id FROM users u, roles r 
WHERE u.phone_number IN ('+8801711223344', '+8801811556677') AND r.name = 'customer'
ON CONFLICT DO NOTHING;

-- User Profiles
INSERT INTO user_profiles (user_id, date_of_birth, gender, blood_group, preferred_language, emergency_contact_name, emergency_contact_phone)
SELECT id, '1988-06-15', 'male', 'B+', 'bn', 'Khadija Ahmed', '+8801711998877' 
FROM users WHERE phone_number = '+8801711223344'
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO user_profiles (user_id, date_of_birth, gender, blood_group, preferred_language, emergency_contact_name, emergency_contact_phone)
SELECT id, '1992-11-20', 'female', 'O+', 'bn', 'Kamrul Hasan', '+8801811443322' 
FROM users WHERE phone_number = '+8801811556677'
ON CONFLICT (user_id) DO NOTHING;

-- Customer Addresses (Protected against duplicate insert on rerun)
INSERT INTO customer_addresses (
    user_id, address_label, recipient_name, recipient_phone,
    division_id, district_id, upazila_id, area_details, postal_code, is_default
)
SELECT 
    u.id, 'Home', 'Tanvir Ahmed', '+8801711223344',
    (SELECT id FROM divisions WHERE code = 'DHAKA'),
    (SELECT id FROM districts WHERE code = 'DHA'),
    (SELECT id FROM upazilas WHERE name = 'Dhanmondi' AND district_id = (SELECT id FROM districts WHERE code = 'DHA')),
    'House 42, Road 9/A, Dhanmondi Residential Area', '1205', TRUE
FROM users u 
WHERE u.phone_number = '+8801711223344'
  AND NOT EXISTS (SELECT 1 FROM customer_addresses ca WHERE ca.user_id = u.id AND ca.recipient_name = 'Tanvir Ahmed');

INSERT INTO customer_addresses (
    user_id, address_label, recipient_name, recipient_phone,
    division_id, district_id, upazila_id, area_details, postal_code, is_default
)
SELECT 
    u.id, 'Home', 'Sultana Razya', '+8801811556677',
    (SELECT id FROM divisions WHERE code = 'DHAKA'),
    (SELECT id FROM districts WHERE code = 'DHA'),
    (SELECT id FROM upazilas WHERE name = 'Gulshan' AND district_id = (SELECT id FROM districts WHERE code = 'DHA')),
    'Apt 5B, Road 113, Gulshan-2', '1212', TRUE
FROM users u 
WHERE u.phone_number = '+8801811556677'
  AND NOT EXISTS (SELECT 1 FROM customer_addresses ca WHERE ca.user_id = u.id AND ca.recipient_name = 'Sultana Razya');

-- 10. Prescriptions & Pharmacist Review Record (Protected against rerun duplicates)
INSERT INTO prescriptions (
    user_id, patient_name, patient_age, patient_gender, doctor_name, hospital_clinic, prescription_date, status, customer_notes, reviewed_by, reviewed_at
)
SELECT 
    u.id, 'Tanvir Ahmed', 38, 'male', 'Prof. Dr. M. A. Jalil', 'Dhaka Medical College Hospital', CURRENT_DATE - 3,
    'approved', 'Please deliver Seclo capsules as prescribed.',
    (SELECT id FROM users WHERE phone_number = '+8801700000002'),
    CURRENT_TIMESTAMP - INTERVAL '2 days'
FROM users u 
WHERE u.phone_number = '+8801711223344'
  AND NOT EXISTS (SELECT 1 FROM prescriptions p WHERE p.user_id = u.id AND p.patient_name = 'Tanvir Ahmed');

-- Prescription Files
INSERT INTO prescription_files (prescription_id, storage_provider, file_storage_key, original_filename, file_size_bytes, mime_type)
SELECT 
    p.id, 'local', 'prescriptions/2026/10/rx_patient_tanvir_42910.pdf', 'prescription_dmch_oct2026.pdf', 1048576, 'application/pdf'
FROM prescriptions p
WHERE p.patient_name = 'Tanvir Ahmed'
  AND NOT EXISTS (SELECT 1 FROM prescription_files pf WHERE pf.prescription_id = p.id)
LIMIT 1;

-- Pharmacist Review Records
INSERT INTO prescription_review_records (prescription_id, pharmacist_id, previous_status, new_status, decision_notes, verified_medicines)
SELECT 
    p.id,
    (SELECT id FROM users WHERE phone_number = '+8801700000002'),
    'submitted', 'approved',
    'Prescription verified against registered BMDC doctor credentials. Seclo 20mg approved for 30 days therapy.',
    '[{"generic": "Omeprazole", "brand": "Seclo", "strength": "20mg", "approved_qty": 30}]'::jsonb
FROM prescriptions p
WHERE p.patient_name = 'Tanvir Ahmed'
  AND NOT EXISTS (SELECT 1 FROM prescription_review_records prr WHERE prr.prescription_id = p.id)
LIMIT 1;

-- 11. Promotional Coupons
INSERT INTO coupons (
    code, title, title_bn, description, discount_type, discount_value, max_discount_amount, min_order_amount,
    usage_limit_per_user, total_usage_limit, valid_from, valid_until, is_active
) VALUES
('NIRAMOY10', 'Welcome 10% Discount', 'স্বাগতম ১০% ছাড়', 'Get 10% off up to 150 BDT on your orders.', 'percentage', 10.00, 150.00, 500.00, 2, 5000, CURRENT_TIMESTAMP - INTERVAL '5 days', CURRENT_TIMESTAMP + INTERVAL '30 days', TRUE),
('HEALTHFIRST', 'Flat 100 BDT Off', '১০০ টাকা নগদ ছাড়', 'Flat 100 BDT off on orders over 1000 BDT.', 'fixed_amount', 100.00, 100.00, 1000.00, 1, 2000, CURRENT_TIMESTAMP - INTERVAL '5 days', CURRENT_TIMESTAMP + INTERVAL '30 days', TRUE),
('FREESHIP', 'Free Dhaka Delivery', 'ফ্রি ডেলিভারি ঢাকা', 'Free standard shipping inside Dhaka for orders over 600 BDT.', 'fixed_amount', 60.00, 60.00, 600.00, 5, 10000, CURRENT_TIMESTAMP - INTERVAL '1 days', CURRENT_TIMESTAMP + INTERVAL '60 days', TRUE)
ON CONFLICT (code) DO NOTHING;

-- 12. Demo Completed Order with Payment & Address Snapshot
INSERT INTO orders (
    order_number, user_id, prescription_id, order_status, payment_status,
    subtotal_amount, discount_amount, delivery_fee, total_amount, currency,
    coupon_code, delivery_method_id, payment_method_id, idempotency_key
)
SELECT 
    'NRM-202610-0001',
    u.id,
    p.id,
    'delivered',
    'paid',
    420.00, 42.00, 60.00, 438.00, 'BDT',
    'NIRAMOY10',
    (SELECT id FROM delivery_methods WHERE code = 'standard_dhaka'),
    (SELECT id FROM payment_methods WHERE code = 'bkash'),
    'ORD-IDEMP-202610-0001'
FROM users u, prescriptions p
WHERE u.phone_number = '+8801711223344' AND p.patient_name = 'Tanvir Ahmed'
LIMIT 1
ON CONFLICT (order_number) DO NOTHING;

-- Order Address Snapshot
INSERT INTO order_addresses (order_id, recipient_name, recipient_phone, division, district, upazila, area_details, postal_code)
SELECT 
    o.id, 'Tanvir Ahmed', '+8801711223344', 'Dhaka', 'Dhaka', 'Dhanmondi', 'House 42, Road 9/A, Dhanmondi Residential Area', '1205'
FROM orders o WHERE o.order_number = 'NRM-202610-0001'
ON CONFLICT (order_id) DO NOTHING;

-- Order Items Snapshot
INSERT INTO order_items (
    order_id, product_id, product_name, product_name_bn, product_sku,
    unit_price_snapshot, quantity, total_price, is_prescription_item
)
SELECT 
    o.id,
    p.id,
    p.name,
    p.name_bn,
    p.sku,
    63.00, 4, 252.00, TRUE
FROM orders o, products p
WHERE o.order_number = 'NRM-202610-0001' AND p.sku = 'MED-SQR-002'
  AND NOT EXISTS (SELECT 1 FROM order_items oi WHERE oi.order_id = o.id AND oi.product_id = p.id);

INSERT INTO order_items (
    order_id, product_id, product_name, product_name_bn, product_sku,
    unit_price_snapshot, quantity, total_price, is_prescription_item
)
SELECT 
    o.id,
    p.id,
    p.name,
    p.name_bn,
    p.sku,
    28.00, 6, 168.00, FALSE
FROM orders o, products p
WHERE o.order_number = 'NRM-202610-0001' AND p.sku = 'MED-SQR-001'
  AND NOT EXISTS (SELECT 1 FROM order_items oi WHERE oi.order_id = o.id AND oi.product_id = p.id);

-- Payment Record for the Completed Order
INSERT INTO payments (
    order_id, payment_method_id, payment_status, amount, currency,
    transaction_id, idempotency_key, gateway_provider, gateway_response, completed_at
)
SELECT 
    o.id,
    (SELECT id FROM payment_methods WHERE code = 'bkash'),
    'captured',
    438.00,
    'BDT',
    'BKSH-TRX-9G8172K19A',
    'PAY-IDEMP-202610-0001',
    'bkash_pgw',
    '{"trxID": "BKSH-TRX-9G8172K19A", "paymentID": "TR001198273", "status": "Success", "amount": "438.00", "currency": "BDT"}'::jsonb,
    CURRENT_TIMESTAMP - INTERVAL '1 day'
FROM orders o WHERE o.order_number = 'NRM-202610-0001'
ON CONFLICT (transaction_id) DO NOTHING;

-- Order Status Timeline
INSERT INTO order_status_history (order_id, previous_status, new_status, notes)
SELECT o.id, NULL, 'pending', 'Order placed by customer.' 
FROM orders o 
WHERE o.order_number = 'NRM-202610-0001'
  AND NOT EXISTS (SELECT 1 FROM order_status_history osh WHERE osh.order_id = o.id)
UNION ALL
SELECT o.id, 'pending', 'confirmed', 'bKash online payment received and verified.' 
FROM orders o 
WHERE o.order_number = 'NRM-202610-0001'
  AND NOT EXISTS (SELECT 1 FROM order_status_history osh WHERE osh.order_id = o.id)
UNION ALL
SELECT o.id, 'confirmed', 'processing', 'Prescription verified by pharmacist; packing initiated.' 
FROM orders o 
WHERE o.order_number = 'NRM-202610-0001'
  AND NOT EXISTS (SELECT 1 FROM order_status_history osh WHERE osh.order_id = o.id)
UNION ALL
SELECT o.id, 'processing', 'shipped', 'Package handed over to Dhaka Metro delivery rider.' 
FROM orders o 
WHERE o.order_number = 'NRM-202610-0001'
  AND NOT EXISTS (SELECT 1 FROM order_status_history osh WHERE osh.order_id = o.id)
UNION ALL
SELECT o.id, 'shipped', 'delivered', 'Order successfully handed to customer at Dhanmondi.' 
FROM orders o 
WHERE o.order_number = 'NRM-202610-0001'
  AND NOT EXISTS (SELECT 1 FROM order_status_history osh WHERE osh.order_id = o.id);

-- 13. Promotional Campaign & Banners
INSERT INTO promotional_campaigns (slug, title, title_bn, description, start_date, end_date) VALUES
('seasonal-healthcare-campaign-2026', 'Winter Wellness & Healthcare Festival', 'শীতকালীন স্বাস্থ্য ও ওয়েলনেস উৎসব', 'Save up to 20% on vitamins, skin moisturizers, and fever care.', CURRENT_TIMESTAMP - INTERVAL '2 days', CURRENT_TIMESTAMP + INTERVAL '28 days')
ON CONFLICT (slug) DO NOTHING;

INSERT INTO banners (campaign_id, title, subtitle, image_url, target_url, display_order)
SELECT 
    id, 'Genuine Medicines Delivered in 2 Hours', 'নিরাপদ ও নির্ভরযোগ্য ওষুধ আপনার দোরগোড়ায়',
    'https://images.niramoyhealth.com.bd/banners/winter-wellness-banner.webp', '/campaigns/winter-wellness', 1
FROM promotional_campaigns 
WHERE slug = 'seasonal-healthcare-campaign-2026'
  AND NOT EXISTS (SELECT 1 FROM banners b WHERE b.campaign_id = promotional_campaigns.id);

-- 14. Product Reviews
INSERT INTO product_reviews (product_id, user_id, rating, title, comment, is_verified_purchase, status)
SELECT 
    p.id, u.id, 5, 'Fast delivery and genuine medicine', 'Received authentic Napa Extra with long expiry date. Delivered within 3 hours in Dhanmondi!', TRUE, 'approved'
FROM products p, users u
WHERE p.sku = 'MED-SQR-001' AND u.phone_number = '+8801711223344'
ON CONFLICT (product_id, user_id) DO NOTHING;

INSERT INTO product_reviews (product_id, user_id, rating, title, comment, is_verified_purchase, status)
SELECT 
    p.id, u.id, 5, 'Highly accurate Omron BP monitor', 'Very user friendly and clear display. Essential device for elderly family members.', TRUE, 'approved'
FROM products p, users u
WHERE p.sku = 'DEV-OMR-001' AND u.phone_number = '+8801811556677'
ON CONFLICT (product_id, user_id) DO NOTHING;

COMMIT;
