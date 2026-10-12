-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 013: SEED REFERENCE DATA
-- =============================================================================
-- Description:
-- Populates essential reference catalogs and constants:
--   - Bangladesh Divisions, Districts, and Major Thanas/Upazilas
--   - System Roles (RBAC)
--   - Delivery & Shipping Methods (with BDT fees)
--   - Bangladesh Payment Gateways & MFS (bKash, Nagad, COD, Rocket, Cards)
--   - Pharmaceutical Dosage Forms (Bilingual)
--   - Core Healthcare & Pharmacy Category Taxonomy (Hierarchical)
--   - Core Application System Settings
-- =============================================================================

BEGIN;

-- 1. Bangladesh Divisions
INSERT INTO divisions (code, name, name_bn, display_order) VALUES
('DHAKA', 'Dhaka', 'ঢাকা', 1),
('CHATTOGRAM', 'Chattogram', 'চট্টগ্রাম', 2),
('RAJSHAHI', 'Rajshahi', 'রাজশাহী', 3),
('KHULNA', 'Khulna', 'খুলনা', 4),
('BARISHAL', 'Barishal', 'বরিশাল', 5),
('SYLHET', 'Sylhet', 'সিলেট', 6),
('RANGPUR', 'Rangpur', 'রংপুর', 7),
('MYMENSINGH', 'Mymensingh', 'ময়মনসিংহ', 8)
ON CONFLICT (code) DO NOTHING;

-- 2. Major Bangladesh Districts
INSERT INTO districts (division_id, code, name, name_bn, display_order) VALUES
((SELECT id FROM divisions WHERE code = 'DHAKA'), 'DHA', 'Dhaka', 'ঢাকা', 1),
((SELECT id FROM divisions WHERE code = 'DHAKA'), 'GAZ', 'Gazipur', 'গাজীপুর', 2),
((SELECT id FROM divisions WHERE code = 'DHAKA'), 'NAR', 'Narayanganj', 'নারায়ণগঞ্জ', 3),
((SELECT id FROM divisions WHERE code = 'CHATTOGRAM'), 'CTG', 'Chattogram', 'চট্টগ্রাম', 4),
((SELECT id FROM divisions WHERE code = 'CHATTOGRAM'), 'COX', 'Cox''s Bazar', 'কক্সবাজার', 5),
((SELECT id FROM divisions WHERE code = 'CHATTOGRAM'), 'COM', 'Cumilla', 'কুমিল্লা', 6),
((SELECT id FROM divisions WHERE code = 'SYLHET'), 'SYL', 'Sylhet', 'সিলেট', 7),
((SELECT id FROM divisions WHERE code = 'RAJSHAHI'), 'RAJ', 'Rajshahi', 'রাজশাহী', 8),
((SELECT id FROM divisions WHERE code = 'RAJSHAHI'), 'BOG', 'Bogura', 'বগুড়া', 9),
((SELECT id FROM divisions WHERE code = 'KHULNA'), 'KHU', 'Khulna', 'খুলনা', 10),
((SELECT id FROM divisions WHERE code = 'BARISHAL'), 'BAR', 'Barishal', 'বরিশাল', 11),
((SELECT id FROM divisions WHERE code = 'RANGPUR'), 'RAN', 'Rangpur', 'রংপুর', 12),
((SELECT id FROM divisions WHERE code = 'MYMENSINGH'), 'MYM', 'Mymensingh', 'ময়মনসিংহ', 13)
ON CONFLICT (code) DO NOTHING;

-- 3. Major Upazilas / Thanas (Dhaka City & Hubs)
INSERT INTO upazilas (district_id, name, name_bn, post_code) VALUES
((SELECT id FROM districts WHERE code = 'DHA'), 'Dhanmondi', 'ধানমন্ডি', '1205'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Gulshan', 'গুলশান', '1212'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Banani', 'বনানী', '1213'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Uttara', 'উত্তরা', '1230'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Mirpur', 'মিরপুর', '1216'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Mohammadpur', 'মোহাম্মদপুর', '1207'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Tejgaon', 'তেজগাঁও', '1208'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Badda', 'বাড্ডা', '1212'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Motijheel', 'মতিঝিল', '1000'),
((SELECT id FROM districts WHERE code = 'DHA'), 'Lalbagh', 'লালবাগ', '1211'),
((SELECT id FROM districts WHERE code = 'CTG'), 'Panchlaish', 'পাঁচলাইশ', '4203'),
((SELECT id FROM districts WHERE code = 'CTG'), 'Kotwali', 'কোতোয়ালী', '4000')
ON CONFLICT (district_id, name) DO NOTHING;

-- 4. User Roles (RBAC)
INSERT INTO roles (name, display_name, description) VALUES
('super_admin', 'Super Administrator', 'Full system control, administrative oversight, and configuration access.'),
('admin', 'Administrator', 'Platform operations, catalog management, inventory oversight.'),
('pharmacist', 'Licensed Pharmacist', 'Prescription validation, Rx verification, and clinical safety approvals.'),
('delivery_agent', 'Delivery Agent', 'Shipment fulfillment, order delivery confirmation, and COD collection.'),
('support_agent', 'Customer Support Agent', 'Customer service ticket resolution and order inquiry support.'),
('customer', 'Customer', 'Registered e-commerce customer placing orders and submitting prescriptions.')
ON CONFLICT (name) DO NOTHING;

-- 5. Delivery Methods
INSERT INTO delivery_methods (code, name, name_bn, base_fee, free_delivery_threshold, estimated_days_min, estimated_days_max) VALUES
('standard_dhaka', 'Standard Delivery (Inside Dhaka)', 'নিয়মিত ডেলিভারি (ঢাকার ভেতরে)', 60.00, 999.00, 1, 2),
('express_dhaka', 'Express Delivery (Inside Dhaka)', 'এক্সপ্রেস ডেলিভারি (ঢাকার ভেতরে - একই দিনে)', 120.00, 1999.00, 0, 1),
('urgent_medicine', 'Urgent Medicine Delivery (Within 2-4 Hours)', 'জরুরি ওষুধ ডেলিভারি (২-৪ ঘণ্টার মধ্যে)', 150.00, NULL, 0, 0),
('outside_dhaka', 'Courier Delivery (Outside Dhaka)', 'কুরিয়ার ডেলিভারি (ঢাকার বাইরে সমগ্র বাংলাদেশ)', 130.00, 2499.00, 2, 4)
ON CONFLICT (code) DO NOTHING;

-- 6. Payment Methods
INSERT INTO payment_methods (code, name, name_bn, is_mfs) VALUES
('cod', 'Cash on Delivery (COD)', 'ক্যাশ অন ডেলিভারি', FALSE),
('bkash', 'bKash (Direct MFS Gateway)', 'বিকাশ', TRUE),
('nagad', 'Nagad (Direct MFS Gateway)', 'নগদ', TRUE),
('rocket', 'DBBL Rocket', 'রকেট', TRUE),
('card_sslcommerz', 'Debit / Credit Card & Net Banking', 'ডেবিট/ক্রেডিট কার্ড ও ইন্টারনেট ব্যাংকিং', FALSE)
ON CONFLICT (code) DO NOTHING;

-- 7. Delivery Zones
INSERT INTO delivery_zones (division_id, district_id, zone_name, estimated_delivery_hours, base_delivery_fee, is_express_available) VALUES
((SELECT id FROM divisions WHERE code = 'DHAKA'), (SELECT id FROM districts WHERE code = 'DHA'), 'Dhaka Metro Urban', 24, 60.00, TRUE),
((SELECT id FROM divisions WHERE code = 'DHAKA'), (SELECT id FROM districts WHERE code = 'GAZ'), 'Gazipur Industrial Zone', 48, 90.00, FALSE),
((SELECT id FROM divisions WHERE code = 'CHATTOGRAM'), (SELECT id FROM districts WHERE code = 'CTG'), 'Chattogram Metro', 48, 120.00, TRUE),
((SELECT id FROM divisions WHERE code = 'SYLHET'), (SELECT id FROM districts WHERE code = 'SYL'), 'Sylhet Metro', 72, 130.00, FALSE)
ON CONFLICT (zone_name) DO NOTHING;

-- 8. Pharmaceutical Dosage Forms
INSERT INTO dosage_forms (name, name_bn) VALUES
('Tablet', 'ট্যাবলেট'),
('Capsule', 'ক্যাপসুল'),
('Syrup', 'সিরাপ'),
('Suspension', 'সাসপেনশন'),
('Injection', 'ইনজেকশন'),
('Ointment', 'মলম'),
('Cream', 'ক্রিম'),
('Gel', 'জেল'),
('Inhaler', 'ইনহেলার'),
('Eye Drops', 'চোখের ড্রপ'),
('Ear Drops', 'কানের ড্রপ'),
('Powder / Sachet', 'পাউডার / স্যাচেট'),
('Suppository', 'সাপোজিটরি'),
('Nebulizer Solution', 'নেবুলাইজার সলিউশন')
ON CONFLICT (name) DO NOTHING;

-- 9. Healthcare & Pharmacy Categories (Root Categories)
INSERT INTO categories (parent_id, slug, name, name_bn, display_order) VALUES
(NULL, 'prescription-medicines', 'Prescription Medicines', 'প্রেসক্রিপশন ওষুধ', 1),
(NULL, 'otc-daily-health', 'OTC & Daily Health', 'ওটিসি ও দৈনন্দিন স্বাস্থ্য', 2),
(NULL, 'medical-devices', 'Medical Devices & Diagnostics', 'চিকিৎসা সরঞ্জাম ও ডায়াগনস্টিক', 3),
(NULL, 'baby-mother-care', 'Baby & Mother Care', 'মা ও শিশুর যত্ন', 4),
(NULL, 'personal-care-hygiene', 'Personal Care & Hygiene', 'ব্যক্তিগত যত্ন ও পরিচ্ছন্নতা', 5),
(NULL, 'nutrition-supplements', 'Nutrition & Supplements', 'পুষ্টি ও খাদ্য সম্পূরক', 6)
ON CONFLICT (slug) DO NOTHING;

-- Subcategories for Prescription Medicines
INSERT INTO categories (parent_id, slug, name, name_bn, display_order) VALUES
((SELECT id FROM categories WHERE slug = 'prescription-medicines'), 'antibiotics', 'Antibiotics & Anti-infectives', 'অ্যান্টিবায়োটিক', 1),
((SELECT id FROM categories WHERE slug = 'prescription-medicines'), 'cardiology-hypertension', 'Cardiology & Blood Pressure', 'হৃদরোগ ও রক্তচাপ', 2),
((SELECT id FROM categories WHERE slug = 'prescription-medicines'), 'diabetes-care', 'Diabetes Care & Insulin', 'ডায়াবেটিস ও ইনসুলিন', 3),
((SELECT id FROM categories WHERE slug = 'prescription-medicines'), 'gastric-antacids', 'Gastrointestinal & Antacids', 'গ্যাস্ট্রিক ও অ্যান্টাসিড', 4),
((SELECT id FROM categories WHERE slug = 'prescription-medicines'), 'respiratory-asthma', 'Respiratory & Asthma', 'শ্বাসকষ্ট ও অ্যাজমা', 5)
ON CONFLICT (slug) DO NOTHING;

-- Subcategories for OTC & Daily Health
INSERT INTO categories (parent_id, slug, name, name_bn, display_order) VALUES
((SELECT id FROM categories WHERE slug = 'otc-daily-health'), 'fever-pain-relief', 'Fever & Pain Relief', 'জ্বর ও ব্যথানাশক', 1),
((SELECT id FROM categories WHERE slug = 'otc-daily-health'), 'cold-cough-allergy', 'Cold, Cough & Allergy', 'সর্দি, কাশি ও অ্যালার্জি', 2),
((SELECT id FROM categories WHERE slug = 'otc-daily-health'), 'first-aid-antiseptics', 'First Aid & Antiseptics', 'ফার্স্ট এইড ও অ্যান্টিসেপ্টিক', 3)
ON CONFLICT (slug) DO NOTHING;

-- Subcategories for Medical Devices
INSERT INTO categories (parent_id, slug, name, name_bn, display_order) VALUES
((SELECT id FROM categories WHERE slug = 'medical-devices'), 'blood-pressure-monitors', 'Blood Pressure Monitors', 'বিপি মনিটর', 1),
((SELECT id FROM categories WHERE slug = 'medical-devices'), 'glucose-meters', 'Blood Glucose Monitors & Strips', 'গ্লুকোমিটার ও স্ট্রিপস', 2),
((SELECT id FROM categories WHERE slug = 'medical-devices'), 'thermometers-nebulizers', 'Thermometers & Nebulizers', 'থার্মোমিটার ও নেবুলাইজার', 3)
ON CONFLICT (slug) DO NOTHING;

-- Subcategories for Baby & Mother Care
INSERT INTO categories (parent_id, slug, name, name_bn, display_order) VALUES
((SELECT id FROM categories WHERE slug = 'baby-mother-care'), 'diapers-wipes', 'Baby Diapers & Wipes', 'ডায়াপার ও বেবি ওয়াইপস', 1),
((SELECT id FROM categories WHERE slug = 'baby-mother-care'), 'baby-food-nutrition', 'Baby Food & Formula', 'শিশুর পুষ্টিকর খাবার', 2),
((SELECT id FROM categories WHERE slug = 'baby-mother-care'), 'maternal-care', 'Maternal & Pregnancy Care', 'মাতৃত্বকালীন যত্ন', 3)
ON CONFLICT (slug) DO NOTHING;

-- 10. Application Configurations
INSERT INTO app_configurations (config_key, config_value, description) VALUES
('SITE_NAME', 'Niramoy Health', 'Brand and platform identity'),
('SITE_NAME_BN', 'নিরাময় হেলথ', 'Bangla platform identity'),
('SUPPORT_HOTLINE', '+8809612000111', 'Customer helpline phone number'),
('SUPPORT_EMAIL', 'support@niramoyhealth.com.bd', 'Official customer support email'),
('EMERGENCY_DELIVERY_ENABLED', 'true', 'Whether urgent emergency medicine delivery is available'),
('MIN_ORDER_AMOUNT_BDT', '100.00', 'Minimum order amount in BDT to checkout'),
('DEFAULT_CURRENCY', 'BDT', 'Primary ISO currency code')
ON CONFLICT (config_key) DO UPDATE SET config_value = EXCLUDED.config_value;

COMMIT;
