import os
import sys
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import parse_xml, OxmlElement
from docx.oxml.ns import nsdecls, qn

def create_proposal_document():
    doc = docx.Document()

    # Pure Black and White Theme Colors
    COLOR_BLACK = RGBColor(0, 0, 0)
    COLOR_DARK_GRAY = RGBColor(55, 65, 81)
    COLOR_MUTED_GRAY = RGBColor(107, 114, 128)

    HEX_BLACK = "000000"
    HEX_DARK_GRAY = "374151"
    HEX_BORDER = "6B7280"
    HEX_LIGHT_BORDER = "D1D5DB"
    HEX_HEADER_BG = "E5E7EB"   # Clean neutral light gray for table headers
    HEX_ZEBRA_BG = "F9FAFB"    # Subtle neutral tint for alternating rows
    HEX_WHITE = "FFFFFF"
    HEX_CALLOUT_BG = "F9FAFB"

    # Set document-level margins (1 inch standard)
    for section in doc.sections:
        section.top_margin = Inches(1.0)
        section.bottom_margin = Inches(1.0)
        section.left_margin = Inches(1.0)
        section.right_margin = Inches(1.0)
        section.page_width = Inches(8.5)
        section.page_height = Inches(11.0)

    # Enable updateFields in Word settings so dynamic fields refresh
    settings = doc.settings.element
    update_fields = parse_xml(f'<w:updateFields {nsdecls("w")} w:val="true"/>')
    settings.append(update_fields)

    # Configure Default Styles
    styles = doc.styles

    # Normal Style (Body text)
    style_normal = styles['Normal']
    font_normal = style_normal.font
    font_normal.name = 'Arial'
    font_normal.size = Pt(10)
    font_normal.color.rgb = COLOR_BLACK
    style_normal.paragraph_format.line_spacing = 1.15
    style_normal.paragraph_format.space_after = Pt(4)

    # Heading 1
    style_h1 = styles['Heading 1']
    font_h1 = style_h1.font
    font_h1.name = 'Arial'
    font_h1.size = Pt(14)
    font_h1.bold = True
    font_h1.color.rgb = COLOR_BLACK
    style_h1.paragraph_format.space_before = Pt(12)
    style_h1.paragraph_format.space_after = Pt(4)
    style_h1.paragraph_format.keep_with_next = True

    # Heading 2
    style_h2 = styles['Heading 2']
    font_h2 = style_h2.font
    font_h2.name = 'Arial'
    font_h2.size = Pt(12)
    font_h2.bold = True
    font_h2.color.rgb = COLOR_BLACK
    style_h2.paragraph_format.space_before = Pt(10)
    style_h2.paragraph_format.space_after = Pt(3)
    style_h2.paragraph_format.keep_with_next = True

    # Heading 3
    style_h3 = styles['Heading 3']
    font_h3 = style_h3.font
    font_h3.name = 'Arial'
    font_h3.size = Pt(10.5)
    font_h3.bold = True
    font_h3.color.rgb = COLOR_BLACK
    style_h3.paragraph_format.space_before = Pt(6)
    style_h3.paragraph_format.space_after = Pt(2)
    style_h3.paragraph_format.keep_with_next = True

    # XML Helper Functions for Table and Cell Styling
    def set_cell_shading(cell, color_hex):
        tcPr = cell._tc.get_or_add_tcPr()
        for child in list(tcPr):
            if child.tag.endswith('shd'):
                tcPr.remove(child)
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{color_hex}"/>')
        tcPr.append(shd)

    def set_cell_margins(cell, top=80, bottom=80, left=100, right=100):
        tcPr = cell._tc.get_or_add_tcPr()
        for child in list(tcPr):
            if child.tag.endswith('tcMar'):
                tcPr.remove(child)
        tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
        tcPr.append(tcMar)

    def set_cell_border(cell, top=None, bottom=None, left=None, right=None):
        tcPr = cell._tc.get_or_add_tcPr()
        for child in list(tcPr):
            if child.tag.endswith('tcBorders'):
                tcPr.remove(child)
        b = f'<w:tcBorders {nsdecls("w")}>'
        for side, spec in [('top', top), ('bottom', bottom), ('left', left), ('right', right)]:
            if spec:
                val, sz, col = spec
                b += f'<w:{side} w:val="{val}" w:sz="{sz}" w:space="0" w:color="{col}"/>'
            else:
                b += f'<w:{side} w:val="none"/>'
        b += '</w:tcBorders>'
        tcPr.append(parse_xml(b))

    def make_header_row(row):
        trPr = row._tr.get_or_add_trPr()
        trPr.append(parse_xml(f'<w:tblHeader {nsdecls("w")}/>'))
        trPr.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))

    def prevent_row_split(row):
        trPr = row._tr.get_or_add_trPr()
        trPr.append(parse_xml(f'<w:cantSplit {nsdecls("w")}/>'))

    # Helper: Add Clean Callout Box (Black and White)
    def add_callout(doc, label, text_paragraphs):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        tbl.autofit = False
        tbl.columns[0].width = Inches(6.5)

        cell = tbl.cell(0, 0)
        set_cell_shading(cell, HEX_CALLOUT_BG)
        set_cell_margins(cell, top=90, bottom=90, left=120, right=120)
        set_cell_border(cell, 
                        left=('single', '24', HEX_BLACK),      # Solid black accent bar
                        top=('single', '4', HEX_LIGHT_BORDER),
                        bottom=('single', '4', HEX_LIGHT_BORDER),
                        right=('single', '4', HEX_LIGHT_BORDER))
        
        p = cell.paragraphs[0]
        p.paragraph_format.space_before = Pt(1)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.line_spacing = 1.15
        r_label = p.add_run(f"{label.upper()}\n")
        r_label.bold = True
        r_label.font.size = Pt(9.5)
        r_label.font.color.rgb = COLOR_BLACK

        for p_text in text_paragraphs:
            p2 = cell.add_paragraph()
            p2.paragraph_format.space_before = Pt(1)
            p2.paragraph_format.space_after = Pt(2)
            p2.paragraph_format.line_spacing = 1.15
            r = p2.add_run(p_text)
            r.font.size = Pt(9)
            r.font.color.rgb = COLOR_BLACK
        
        p_space = doc.add_paragraph()
        p_space.paragraph_format.space_before = Pt(1)
        p_space.paragraph_format.space_after = Pt(2)

    # Helper: Add Clean Black and White Table
    def add_table(doc, headers, data, col_widths=None, compact=False):
        tbl = doc.add_table(rows=len(data) + 1, cols=len(headers))
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        tbl.autofit = False

        if col_widths:
            for i, w in enumerate(col_widths):
                tbl.columns[i].width = Inches(w)

        # Header Row
        hdr_row = tbl.rows[0]
        make_header_row(hdr_row)
        c_top = 50 if compact else 80
        c_bot = 50 if compact else 80
        c_side = 70 if compact else 100
        f_size = Pt(8.5) if compact else Pt(9.0)
        f_dsize = Pt(8.0) if compact else Pt(8.5)

        for i, h_text in enumerate(headers):
            cell = hdr_row.cells[i]
            if col_widths:
                cell.width = Inches(col_widths[i])
            set_cell_shading(cell, HEX_HEADER_BG)
            set_cell_margins(cell, top=c_top, bottom=c_bot, left=c_side, right=c_side)
            set_cell_border(cell, 
                            top=('single', '8', HEX_BLACK),
                            bottom=('single', '8', HEX_BLACK),
                            left=('none', '0', HEX_WHITE),
                            right=('none', '0', HEX_WHITE))
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            run = p.add_run(h_text)
            run.bold = True
            run.font.size = f_size
            run.font.color.rgb = COLOR_BLACK

        # Data Rows
        for r_idx, row_data in enumerate(data):
            row = tbl.rows[r_idx + 1]
            prevent_row_split(row)
            bg = HEX_ZEBRA_BG if r_idx % 2 == 1 else HEX_WHITE

            for c_idx, cell_value in enumerate(row_data):
                cell = row.cells[c_idx]
                if col_widths:
                    cell.width = Inches(col_widths[c_idx])
                set_cell_shading(cell, bg)
                set_cell_margins(cell, top=c_top, bottom=c_bot, left=c_side, right=c_side)
                
                is_last_row = (r_idx == len(data) - 1)
                b_bottom = ('single', '8', HEX_BLACK) if is_last_row else ('single', '4', HEX_LIGHT_BORDER)
                set_cell_border(cell,
                                top=('none', '0', HEX_WHITE),
                                bottom=b_bottom,
                                left=('none', '0', HEX_WHITE),
                                right=('none', '0', HEX_WHITE))

                p = cell.paragraphs[0]
                p.alignment = WD_ALIGN_PARAGRAPH.LEFT
                p.paragraph_format.space_before = Pt(0)
                p.paragraph_format.space_after = Pt(0)
                p.paragraph_format.line_spacing = 1.15
                run = p.add_run(str(cell_value))
                run.font.size = f_dsize
                run.font.color.rgb = COLOR_BLACK

        p_space = doc.add_paragraph()
        p_space.paragraph_format.space_before = Pt(1)
        p_space.paragraph_format.space_after = Pt(2)

    def add_body(doc, text):
        p = doc.add_paragraph()
        p.paragraph_format.line_spacing = 1.15
        p.paragraph_format.space_after = Pt(4)
        run = p.add_run(text)
        run.font.size = Pt(10)
        run.font.color.rgb = COLOR_BLACK
        return p

    def add_bullet(doc, bold_prefix, text):
        p = doc.add_paragraph(style='List Bullet')
        p.paragraph_format.line_spacing = 1.15
        p.paragraph_format.space_before = Pt(1)
        p.paragraph_format.space_after = Pt(2)
        r_bold = p.add_run(bold_prefix)
        r_bold.bold = True
        r_bold.font.size = Pt(9.5)
        r_bold.font.color.rgb = COLOR_BLACK
        r_text = p.add_run(text)
        r_text.font.size = Pt(9.5)
        r_text.font.color.rgb = COLOR_BLACK
        return p

    print("Generating Title Block...")

    # Document Header / Title
    title_p = doc.add_paragraph()
    title_p.paragraph_format.space_before = Pt(18)
    title_p.paragraph_format.space_after = Pt(2)
    r_title = title_p.add_run("PROJECT PROPOSAL: NIRAMOY HEALTH (নিরাময় হেলথ)")
    r_title.bold = True
    r_title.font.size = Pt(18)
    r_title.font.color.rgb = COLOR_BLACK

    sub_p = doc.add_paragraph()
    sub_p.paragraph_format.space_before = Pt(0)
    sub_p.paragraph_format.space_after = Pt(12)
    r_sub = sub_p.add_run("Enterprise Digital Healthcare & E-Pharmacy Commerce Platform for Bangladesh")
    r_sub.font.size = Pt(11)
    r_sub.font.color.rgb = COLOR_DARK_GRAY

    # Metadata Summary Table
    meta_headers = ["Project Parameter", "Specification & Details"]
    meta_data = [
        ("Project Title", "Niramoy Health (নিরাময় হেলথ)"),
        ("Industry Domain", "Digital Pharmacy, Healthcare E-Commerce & Tele-prescription Fulfillment"),
        ("Primary Target Market", "Bangladesh (Dhaka Metro Express & Nationwide 64 Districts)"),
        ("Database Architecture", "PostgreSQL 16+ (54 Tables, 6 Operational Views, 655 Constraints) — STATUS: COMPLETED"),
        ("Frontend Application", "Vite 8.3 + Tailwind CSS 3.4 (SPA, Responsive across 320px–1440px) — STATUS: COMPLETED"),
        ("Backend Services", "ASP.NET Core (.NET 10) Clean Architecture API — STATUS: IN PROGRESS (INCOMPLETE)"),
        ("Document Purpose", "Formal technical proposal, module inventory, gap analysis, and implementation roadmap")
    ]
    add_table(doc, meta_headers, meta_data, col_widths=[2.2, 4.3])

    add_callout(doc, "Project Status Notice", [
        "Current System Completion State: The Database Architecture and the Frontend Storefront/Admin Portal are 100% completed, verified, and audited.",
        "The Backend API has its architectural scaffolding, domain entities, and data access layers implemented, but business services, live MFS payment integrations (bKash/Nagad), SMS gateway connectors, cloud storage with signed URLs, and background schedulers remain in progress (incomplete)."
    ])

    # ==========================================
    # SECTION 1: EXECUTIVE SUMMARY
    # ==========================================
    doc.add_heading("1. Executive Summary", level=1)
    add_body(doc, "Niramoy Health is a specialized digital pharmacy and healthcare e-commerce platform engineered specifically for the healthcare retail market in Bangladesh. Unlike conventional online retail stores, a digital pharmacy requires strict regulatory adherence, rigorous prescription validation, temperature-controlled batch tracking, and localized delivery workflows.")
    add_body(doc, "The project addresses core challenges in the Bangladeshi pharmaceutical supply chain: the circulation of unverified medicines, lack of digital prescription management, stock inaccuracies during high-demand periods, and difficulties in addressing and dispatching outside major metropolitan areas.")
    add_body(doc, "This proposal provides an exhaustive overview of the system's operational architecture, technical foundation, completed deliverables, feature catalogs, and the concrete implementation plan required to complete the remaining backend API services.")

    # ==========================================
    # SECTION 2: PROBLEM STATEMENT & MARKET FIT
    # ==========================================
    doc.add_heading("2. Problem Statement & Bangladesh Market Fit", level=1)
    add_body(doc, "The retail pharmacy ecosystem in Bangladesh serves over 170 million people but remains heavily reliant on physical, fragmented, and largely unverified local pharmacies. Several acute operational and technical challenges persist:")

    add_bullet(doc, "Counterfeit Medicine Sourcing: ", "Counterfeit and improperly stored pharmaceuticals circulate widely in unorganized retail markets. Patients have no systematic method to verify genuine manufacturer sourcing, batch details, or expiration dates.")
    add_bullet(doc, "Dispensing Without Valid Prescriptions: ", "Schedule drugs and antibiotics are frequently sold over the counter without legitimate doctor prescriptions, leading to severe antibiotic resistance and public health risks. A compliant platform must enforce a clinical gatekeeping triage before orders can be fulfilled.")
    add_bullet(doc, "Stock Inaccuracies & Overselling: ", "Standard e-commerce engines lack two-phase inventory reservation. When multiple customers attempt to purchase vital medicines simultaneously during sudden surges, overselling occurs if stock reservation is not strictly managed at the database engine level.")
    add_bullet(doc, "Addressing & Logistics Complexities: ", "Conventional delivery tools assume standardized postal codes and street names. In Bangladesh, delivery addressing requires a strict administrative hierarchy (8 Divisions, 64 Districts, 495+ Upazilas/Thanas) paired with localized street instructions.")
    add_bullet(doc, "Payment Method Preferences: ", "Over 80% of digital transactions in Bangladesh occur via Mobile Financial Services (bKash, Nagad) or Cash on Delivery (COD). A rigid credit card only checkout process cannot succeed.")

    # ==========================================
    # SECTION 3: SYSTEM ARCHITECTURE
    # ==========================================
    doc.add_heading("3. System Architecture & Technology Stack", level=1)
    add_body(doc, "Niramoy Health employs a decoupled three-tier architecture ensuring maintainability, data integrity, and cross-platform performance:")

    arch_headers = ["Layer", "Technology", "Status", "Core Purpose"]
    arch_data = [
        ("Database Engine", "PostgreSQL 16+", "Completed", "Authoritative relational source of truth, 54 tables, strict checks, trigram search."),
        ("Frontend Client", "Vite 8.3 + Tailwind CSS 3.4", "Completed", "Lightweight, responsive Single Page Application (SPA), role-guarded views, guest cart."),
        ("Backend Services", "ASP.NET Core (.NET 10)", "In Progress", "Clean Architecture REST API, business logic validation, JWT authentication."),
        ("File Storage", "Local Disk -> S3 Storage", "In Progress", "Prescription image uploads, moving from local staging to private cloud object buckets."),
        ("Payment Connectors", "bKash & Nagad Gateways", "Incomplete", "MFS token grant, checkout URL redirection, IPN callback verification.")
    ]
    add_table(doc, arch_headers, arch_data, col_widths=[1.4, 1.8, 1.0, 2.3])

    # ==========================================
    # SECTION 4: CURRENT STATUS & GAP ANALYSIS
    # ==========================================
    doc.add_heading("4. Current Implementation Status & Technical Gap Analysis", level=1)
    add_body(doc, "To establish clear milestones for stakeholders and technical evaluators, the table below provides an exact breakdown of what has been finalized versus what is still under construction:")

    status_headers = ["Module & Layer", "Deliverable Description", "Current State", "Engineering Notes"]
    status_data = [
        ("Database: Core Tables", "54 tables across Auth, Catalog, Inventory, Rx, Orders, Delivery", "Completed", "Fully migrated, foreign keys, UTF-8 collation, rollback scripts ready."),
        ("Database: Constraints", "655 check constraints, unique constraints, and composite indexes", "Completed", "Enforces non-negative prices, quantity_reserved <= on_hand, valid expiry."),
        ("Database: Views & Seed", "6 operational reporting views, reference seed for 64 districts", "Completed", "Seeded with genuine Bangladeshi medicines (Square, Beximco, Incepta)."),
        ("Database: Automated Tests", "15-point PostgreSQL schema integrity test runner", "Completed", "All 15 database automated tests passing successfully."),
        ("Frontend: Storefront UI", "Homepage, Catalog, Product Details, Category Nav, Search", "Completed", "Debounced bilingual search, responsive 320px–1440px layout."),
        ("Frontend: Rx Portal", "Multi-file drag-and-drop (.jpg, .png, .pdf) & queue tracking", "Completed", "Client-side validation, patient details modal, status history."),
        ("Frontend: Cart & Checkout", "Cart drawer, guest merge, BD address selector, MFS options", "Completed", "Reactive pub/sub stores, guest token retention, payment selector."),
        ("Frontend: Admin Portal", "Pharmacist prescription lightbox review, order dispatch pipeline", "Completed", "Approve/Reject actions, stock threshold warnings, role guards."),
        ("Backend: Scaffolding", "Clean Architecture solution (.slnx), Domain entities, DTOs", "Completed", "Compiles cleanly with 0 warnings and 0 errors."),
        ("Backend: DbContext", "EF Core DbContext mapping and PostgreSQL entity configurations", "Completed", "Entities and relationships mapped to PostgreSQL schema."),
        ("Backend: Live MFS", "bKash and Nagad production API integration and IPN callbacks", "Incomplete", "Currently mocked for flow testing; live gateway integration pending."),
        ("Backend: SMS Gateway", "OTP phone authentication and delivery status SMS alerts", "Incomplete", "Local SMS gateway connectors (e.g., SSL Wireless) pending."),
        ("Backend: Private Storage", "Cloud S3 bucket integration with short-lived signed URLs", "Incomplete", "Currently saves to local disk; cloud security isolation needed."),
        ("Backend: Background Jobs", "Quartz/Hangfire workers for cart release and expiry scans", "Incomplete", "Scheduler for releasing 30-min reserved stock pending."),
        ("Backend: Real-time Hubs", "SignalR live updates for pharmacist review queue", "Incomplete", "Live WebSockets for instant pharmacist alerts pending.")
    ]
    add_table(doc, status_headers, status_data, col_widths=[1.5, 2.3, 1.0, 1.7], compact=True)

    # ==========================================
    # SECTION 5: DETAILED FEATURE LIST
    # ==========================================
    doc.add_heading("5. Detailed Feature List", level=1)
    
    doc.add_heading("5.1 Customer Storefront & Catalog Experience", level=2)
    add_bullet(doc, "Bilingual Drug Discovery: ", "Debounced search queries both English brand names and Bengali script (বাংলা) using PostgreSQL trigram fuzzy matching (pg_trgm).")
    add_bullet(doc, "Faceted Catalog Filtering: ", "Filter medicines by therapeutic category, manufacturer (Square, Beximco, Renata, Incepta), dosage form (Tablet, Capsule, Syrup), and prescription necessity.")
    add_bullet(doc, "Generic Drug Substitution: ", "Product detail pages display active generic formulations and link directly to alternative brands with identical therapeutic chemical compositions.")
    add_bullet(doc, "Transparent Pricing & Sizing: ", "Clear breakdown between strip price and box/pack price, with live discount calculations in Bangladeshi Taka (৳).")

    doc.add_heading("5.2 Clinical Prescription Management", level=2)
    add_bullet(doc, "Multi-Format Upload: ", "Customers can drag-and-drop or photograph prescriptions (.jpg, .jpeg, .png, .pdf) up to 10MB per attachment.")
    add_bullet(doc, "Patient Detail Capture: ", "Forms collect patient name, contact phone, delivery address, and clinical guidance for the pharmacist.")
    add_bullet(doc, "Transparent Triage States: ", "Real-time visual queue displaying status: Submitted -> Under Review -> Approved -> Rejected (with pharmacist remarks).")

    doc.add_heading("5.3 Shopping Cart & Localized Checkout", level=2)
    add_bullet(doc, "Persistent Guest Cart Merge: ", "Unauthenticated visitors can build a cart saved via local guest tokens; upon login, the cart seamlessly merges into their user account.")
    add_bullet(doc, "Clinical Cart Warning: ", "Cart drawer automatically identifies prescription-required medicines and instructs the user to provide an approved prescription.")
    add_bullet(doc, "Bangladeshi Address Hierarchy: ", "Cascading selectors validate administrative divisions, districts, and upazilas/thanas to eliminate delivery dispatch errors.")
    add_bullet(doc, "Flexible Delivery & Payment: ", "Supports Dhaka Metro Express (2–4 hours) and Standard nationwide shipping, with Cash on Delivery, bKash, and Nagad.")

    doc.add_heading("5.4 Pharmacy Staff & Administration Portal", level=2)
    add_bullet(doc, "Pharmacist Review Lightbox: ", "Staff can inspect high-resolution prescription images, verify BMDC doctor registration, and validate dosages.")
    add_bullet(doc, "One-Click Decision Flow: ", "Pharmacists can approve items or reject them with mandatory structured explanation messages sent to the patient.")
    add_bullet(doc, "Order Dispatch Pipeline: ", "Warehouse staff can advance orders across life stages: Placed -> Verified -> Packed -> Out for Delivery -> Delivered.")
    add_bullet(doc, "Inventory Threshold Alerts: ", "Live warnings flag products that have reached or breached minimum safety reorder thresholds.")

    # ==========================================
    # SECTION 6: DATABASE ARCHITECTURE (COMPLETED)
    # ==========================================
    doc.add_heading("6. Completed Database Architecture", level=1)
    add_body(doc, "The database layer is fully built and verified in PostgreSQL 16+. It serves as the authoritative engine enforcing all transactional rules:")

    db_headers = ["Domain Schema", "Table Count", "Key Tables", "Integrity & Compliance Mechanisms"]
    db_data = [
        ("Auth & Users", "9", "users, roles, user_profiles, user_sessions, customer_addresses", "BCrypt hash storage, RBAC roles, UUID public IDs, session tracking."),
        ("Geography", "3", "divisions, districts, upazilas", "Pre-seeded reference hierarchy for all 8 divisions and 64 districts."),
        ("Catalog", "10", "categories, products, generics, brands, manufacturers, dosage_forms", "Trigram GIN indexes for fast fuzzy search, bilingual name columns."),
        ("Inventory", "4", "warehouses, inventory, medicine_batches, inventory_transactions", "Two-phase reservation, audit ledger, batch expiry constraint."),
        ("Cart & Wishlist", "4", "carts, cart_items, wishlists, wishlist_items", "Guest token linkage, dynamic recalculation, line item tracking."),
        ("Prescriptions", "3", "prescriptions, prescription_files, prescription_review_records", "Multi-file attachments, pharmacist decision audit log."),
        ("Orders & Pay", "7", "orders, order_items, order_addresses, payments, refunds", "Immutable price snapshots, immutable address snapshots, strict math checks."),
        ("Delivery & Promo", "8", "delivery_methods, payment_methods, coupons, campaigns, banners", "Zone-based shipping rates, voucher constraints, CMS sections."),
        ("Support & Audit", "6", "support_tickets, notifications, audit_logs, app_configurations", "Structured audit trail, administrative action logging, preferences.")
    ]
    add_table(doc, db_headers, db_data, col_widths=[1.4, 0.8, 2.3, 2.0])

    add_body(doc, "Key Architectural Controls Built into the Database:")
    add_bullet(doc, "Two-Phase Reservation: ", "The inventory table tracks quantity_on_hand and quantity_reserved. A database check constraint enforces CHECK (quantity_reserved <= quantity_on_hand), eliminating overselling at the hardware level.")
    add_bullet(doc, "Immutable Historical Records: ", "Future price changes never modify existing receipts. When an order is placed, order_items saves an immutable snapshot of unit price, SKU, English name, and Bengali name.")
    add_bullet(doc, "Automated Verification: ", "The schema includes 15 automated validation test suites (tests/database/run_tests.py) verifying foreign keys, triggers, constraints, and operational views.")

    # ==========================================
    # SECTION 7: FRONTEND STOREFRONT (COMPLETED)
    # ==========================================
    doc.add_heading("7. Completed Frontend Storefront & Admin Portal", level=1)
    add_body(doc, "The frontend application is fully built, tested, and audited. Built using Vite 8.3 and Tailwind CSS 3.4, it delivers high-performance client rendering without the overhead of heavy JavaScript frameworks:")

    fe_headers = ["View / Module", "File Location", "Key Implemented Capabilities"]
    fe_data = [
        ("Storefront Homepage", "src/pages/HomePage.js", "Hero banner, Rx upload CTA, category pills, discount carousel, trust badges."),
        ("Catalog & Search", "src/pages/CatalogPage.js", "Debounced search, faceted sidebar filters (brand, form, Rx), sort dropdown."),
        ("Product Details", "src/pages/ProductDetailsPage.js", "Bilingual names, generic formula links, pack pricing, strip stepper."),
        ("Prescription Upload", "src/pages/PrescriptionUploadPage.js", "Drag-and-drop file upload (.jpg, .png, .pdf), patient modal, validation."),
        ("Prescription List", "src/pages/PrescriptionsListPage.js", "Customer Rx queue with status badges (Submitted, Approved, Rejected)."),
        ("Checkout & Delivery", "src/pages/CheckoutPage.js", "BD Division/District dropdown, Express/Standard shipping, MFS selection."),
        ("Order Confirmation", "src/pages/OrderConfirmationPage.js", "Official order receipt, breakdown, estimated delivery schedule."),
        ("Order Tracking", "src/pages/OrderDetailsPage.js", "Visual 5-step status progression, line item pricing, printable invoice."),
        ("Admin Dashboard", "src/pages/AdminDashboardPage.js", "Prescription review lightbox, approve/reject triage, order dispatch pipeline."),
        ("Global Navigation", "src/components/Header.js", "Bilingual branding, live debounced search autocomplete, cart badge."),
        ("Mobile Bottom Nav", "src/components/BottomNav.js", "Persistent thumb navigation bar for mobile users (Home, Catalog, Rx, Cart)."),
        ("Cart Drawer", "src/components/CartDrawer.js", "Slide-over panel, increment/decrement controls, Rx advisory notice.")
    ]
    add_table(doc, fe_headers, fe_data, col_widths=[1.5, 2.0, 3.0], compact=True)

    add_body(doc, "Mobile Responsiveness Audit:")
    add_body(doc, "The frontend was tested across 320px (compact smartphones), 375px (standard mobile), 768px (tablets), and 1280px+ (desktop). Touch targets adhere to 44px minimum sizing, forms maintain readable font sizes to avoid mobile browser auto-zoom, and layout shifts (CLS) are strictly minimized.")

    # ==========================================
    # SECTION 8: BACKEND API (IN PROGRESS)
    # ==========================================
    doc.add_heading("8. In-Progress Backend API & Technical Roadmap", level=1)
    add_body(doc, "The backend API is developed using ASP.NET Core on .NET 10 following Clean Architecture principles. While the foundational scaffolding compiles with 0 errors, production connectors and transactional handlers are currently in progress (incomplete).")

    add_body(doc, "Remaining Engineering Work Required to Complete the Backend:")
    
    add_bullet(doc, "1. Live Mobile Financial Services Integration: ", "The current system uses mocked payment endpoints for checkout simulation. Production integration requires connecting with official bKash and Nagad merchant APIs, handling security certificates, checkout URL redirection, IPN callback webhooks, and query transaction verification.")
    add_bullet(doc, "2. Cloud Storage Migration & Prescription Privacy: ", "Prescription files are currently stored on the local server disk. Incomplete work includes configuring an S3-compatible cloud bucket (AWS S3 or Azure Blob) and generating short-lived, pre-signed temporary URLs (expiring in 5–10 minutes) so medical images are never publicly accessible.")
    add_bullet(doc, "3. Local Bangladeshi SMS Gateway: ", "Integration with domestic telecom SMS providers (e.g., SSL Wireless, Greenweb) is required for OTP phone authentication, password resets, and automated SMS alerts upon order confirmation and dispatch.")
    add_bullet(doc, "4. Background Schedulers (Quartz.NET / Hangfire): ", "Implement background workers to: (a) automatically cancel and release reserved inventory for carts abandoned past 30 minutes, (b) scan warehouse inventory daily for batches nearing expiration within 90 days, and (c) generate daily financial summaries.")
    add_bullet(doc, "5. Real-Time WebSockets (SignalR): ", "Implement SignalR hubs to push live notifications to the pharmacist triage queue when a new prescription is submitted, and broadcast order status updates to active customer screens.")

    # ==========================================
    # SECTION 9: IMPLEMENTATION ROADMAP
    # ==========================================
    doc.add_heading("9. Project Implementation Roadmap & Milestones", level=1)
    add_body(doc, "The structured roadmap below details the timeline and phases necessary to finalize the backend and launch the complete platform:")

    roadmap_headers = ["Phase", "Focus Area", "Current Status", "Key Deliverables"]
    roadmap_data = [
        ("Phase 1", "Database Architecture", "Completed", "54 tables, 6 views, 655 constraints, seed data, health checks."),
        ("Phase 2", "Frontend Storefront", "Completed", "Responsive Vite SPA, catalog, cart, Rx upload, admin dashboard."),
        ("Phase 3", "Backend Core API", "Active (In Progress)", "Finalizing transaction pipelines in OrderService and CartService."),
        ("Phase 4", "External Connectors", "Upcoming", "bKash & Nagad live APIs, SMS gateway, private S3 bucket configuration."),
        ("Phase 5", "Background Workers", "Upcoming", "Quartz/Hangfire workers for cart stock release and batch expiry alerts."),
        ("Phase 6", "End-to-End Testing", "Upcoming", "Load testing, concurrency checkout stress testing, production deployment.")
    ]
    add_table(doc, roadmap_headers, roadmap_data, col_widths=[1.1, 1.8, 1.4, 2.2])

    # ==========================================
    # SECTION 10: GOVERNANCE & RISK MITIGATION
    # ==========================================
    doc.add_heading("10. Project Governance & Risk Management", level=1)
    add_body(doc, "Every healthcare software system must anticipate operational and technical risks. The table below details how Niramoy Health mitigates key concerns:")

    risk_headers = ["Identified Risk", "Impact", "Likelihood", "Mitigation Strategy"]
    risk_data = [
        ("MFS Merchant Approval Delays", "Medium", "High", "Launch with Cash on Delivery first. The payment gateway interface is modular, allowing bKash/Nagad to be activated immediately once credentials are provided."),
        ("Prescription Privacy Breaches", "High", "Low", "Store files in private, non-public cloud buckets with strict temporary pre-signed URLs expiring within minutes."),
        ("Inventory Lock Contention", "Medium", "Low", "PostgreSQL two-phase reservation with indexed row locks ensures fast, non-blocking order placement during peak traffic surges."),
        ("SMS Gateway Downtime", "Medium", "Medium", "Implement automated email notifications as a reliable fallback, and queue SMS dispatch with exponential backoff retry logic."),
        ("Regulatory Dispensing Violations", "High", "Low", "Enforce strict pharmacist approval gates on all prescription medicines with mandatory BMDC registration logs.")
    ]
    add_table(doc, risk_headers, risk_data, col_widths=[1.8, 0.8, 0.9, 3.0])

    # ==========================================
    # SECTION 11: CONCLUSION
    # ==========================================
    doc.add_heading("11. Conclusion & Recommendations", level=1)
    add_body(doc, "Niramoy Health is a robust, well-architected software project designed specifically to meet the digital pharmacy and healthcare delivery requirements of Bangladesh.")
    add_body(doc, "By prioritizing the database and frontend storefront first, the project has established an exceptionally strong data foundation—featuring 54 relational tables, strict clinical constraints, and 15 passing automated health checks—alongside an audited, lightweight, mobile-first user experience.")
    add_body(doc, "With the frontend and database completely finalized, engineering efforts are now focused on completing the remaining backend services: live bKash/Nagad payment gateways, SMS delivery, cloud storage isolation, and background job automation. Once these backend tasks are completed, Niramoy Health will be fully ready for deployment as an enterprise-grade digital healthcare platform.")

    add_callout(doc, "Recommended Next Steps", [
        "1. Prioritize Backend Milestone 1: Finalize database transactional pipelines in OrderService and CartService.",
        "2. Provision Gateway Credentials: Secure sandbox and live merchant API keys for bKash and Nagad.",
        "3. Cloud Bucket Setup: Establish private cloud storage buckets with time-limited pre-signed URL generation.",
        "4. Deploy and Verify: Execute end-to-end integration test suites connecting the completed frontend to the finalized backend."
    ])

    # Save to project root
    output_filename = "Niramoy_Health_Project_Proposal.docx"
    doc.save(output_filename)
    print(f"Proposal successfully saved to: {output_filename}")
    return output_filename

if __name__ == "__main__":
    create_proposal_document()
