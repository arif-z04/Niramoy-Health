import os
import sys
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml import parse_xml, OxmlElement
from docx.oxml.ns import nsdecls, qn

def build_proposal_document():
    doc = docx.Document()

    # Define Color Palette
    HEX_NAVY = "1B365D"       # Primary Accent / Headers
    HEX_TEAL = "0D9488"       # Secondary Accent / Subheaders
    HEX_CHARCOAL = "1E293B"   # Body Text
    HEX_MUTED = "64748B"      # Captions & Notes
    HEX_BORDER = "CBD5E1"     # Table Borders
    HEX_ZEBRA = "F8FAFC"      # Alternating Table Row
    HEX_AMBER = "D97706"      # Warning / Notice
    HEX_CALLOUT_BG = "F0FDF4" # Safe / Callout BG
    HEX_WARN_BG = "FFFBEB"    # Warn Callout BG
    HEX_BLUE_BG = "F0F9FF"    # Info Callout BG

    COLOR_NAVY = RGBColor(27, 54, 93)
    COLOR_TEAL = RGBColor(13, 148, 136)
    COLOR_CHARCOAL = RGBColor(30, 41, 59)
    COLOR_MUTED = RGBColor(100, 116, 139)

    # Set document-level margins (1 inch)
    for section in doc.sections:
        section.top_margin = Inches(1.0)
        section.bottom_margin = Inches(1.0)
        section.left_margin = Inches(1.0)
        section.right_margin = Inches(1.0)
        section.page_width = Inches(8.5)
        section.page_height = Inches(11.0)

    # Enable updateFields in settings
    settings = doc.settings.element
    update_fields = parse_xml(f'<w:updateFields {nsdecls("w")} w:val="true"/>')
    settings.append(update_fields)

    # Style Configurations
    styles = doc.styles

    # Normal Style
    style_normal = styles['Normal']
    font_normal = style_normal.font
    font_normal.name = 'Arial'
    font_normal.size = Pt(10.5)
    font_normal.color.rgb = COLOR_CHARCOAL
    style_normal.paragraph_format.line_spacing = 1.15
    style_normal.paragraph_format.space_after = Pt(4)

    # Heading 1
    style_h1 = styles['Heading 1']
    font_h1 = style_h1.font
    font_h1.name = 'Arial'
    font_h1.size = Pt(17)
    font_h1.bold = True
    font_h1.color.rgb = COLOR_NAVY
    style_h1.paragraph_format.space_before = Pt(14)
    style_h1.paragraph_format.space_after = Pt(4)
    style_h1.paragraph_format.keep_with_next = True

    # Heading 2
    style_h2 = styles['Heading 2']
    font_h2 = style_h2.font
    font_h2.name = 'Arial'
    font_h2.size = Pt(13.5)
    font_h2.bold = True
    font_h2.color.rgb = COLOR_TEAL
    style_h2.paragraph_format.space_before = Pt(11)
    style_h2.paragraph_format.space_after = Pt(3)
    style_h2.paragraph_format.keep_with_next = True

    # Heading 3
    style_h3 = styles['Heading 3']
    font_h3 = style_h3.font
    font_h3.name = 'Arial'
    font_h3.size = Pt(11.5)
    font_h3.bold = True
    font_h3.color.rgb = COLOR_CHARCOAL
    style_h3.paragraph_format.space_before = Pt(8)
    style_h3.paragraph_format.space_after = Pt(2)
    style_h3.paragraph_format.keep_with_next = True

    # Heading 4
    style_h4 = styles['Heading 4']
    font_h4 = style_h4.font
    font_h4.name = 'Arial'
    font_h4.size = Pt(10.5)
    font_h4.bold = True
    font_h4.italic = True
    font_h4.color.rgb = COLOR_MUTED
    style_h4.paragraph_format.space_before = Pt(6)
    style_h4.paragraph_format.space_after = Pt(2)
    style_h4.paragraph_format.keep_with_next = True

    # Helper XML functions
    def set_cell_shading(cell, color_hex):
        tcPr = cell._tc.get_or_add_tcPr()
        for child in list(tcPr):
            if child.tag.endswith('shd'):
                tcPr.remove(child)
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{color_hex}"/>')
        tcPr.append(shd)

    def set_cell_margins(cell, top=120, bottom=120, left=150, right=150):
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

    # Helper: Add Callout Box
    def add_callout(doc, title, text_items, kind="info"):
        bg_color = HEX_BLUE_BG if kind == "info" else (HEX_WARN_BG if kind == "warning" else HEX_CALLOUT_BG)
        border_color = HEX_NAVY if kind == "info" else (HEX_AMBER if kind == "warning" else HEX_TEAL)
        
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        tbl.autofit = False
        tbl.columns[0].width = Inches(6.5)

        cell = tbl.cell(0, 0)
        set_cell_shading(cell, bg_color)
        set_cell_margins(cell, top=140, bottom=140, left=180, right=160)
        set_cell_border(cell, 
                        left=('single', '32', border_color),
                        top=('single', '4', HEX_BORDER),
                        bottom=('single', '4', HEX_BORDER),
                        right=('single', '4', HEX_BORDER))
        
        p = cell.paragraphs[0]
        p.paragraph_format.space_before = Pt(1)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.line_spacing = 1.15
        r_title = p.add_run(f"📌 {title.upper()}\n" if kind=="info" else (f"⚠️ {title.upper()}\n" if kind=="warning" else f"💡 {title.upper()}\n"))
        r_title.bold = True
        r_title.font.size = Pt(9.5)
        r_title.font.color.rgb = RGBColor.from_string(border_color)

        for i, item in enumerate(text_items):
            p2 = cell.add_paragraph()
            p2.paragraph_format.space_before = Pt(1)
            p2.paragraph_format.space_after = Pt(1)
            p2.paragraph_format.line_spacing = 1.15
            r = p2.add_run(item)
            r.font.size = Pt(8.5)
            r.font.color.rgb = COLOR_CHARCOAL
        
        p_space = doc.add_paragraph()
        p_space.paragraph_format.space_before = Pt(1)
        p_space.paragraph_format.space_after = Pt(2)

    # Helper: Add Styled Table
    def add_styled_table(doc, headers, data, col_widths=None, align_center=True, compact=False):
        tbl = doc.add_table(rows=len(data) + 1, cols=len(headers))
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER if align_center else WD_TABLE_ALIGNMENT.LEFT
        tbl.autofit = False

        # Set column widths
        if col_widths:
            for i, w in enumerate(col_widths):
                tbl.columns[i].width = Inches(w)

        # Style Header Row
        hdr_row = tbl.rows[0]
        make_header_row(hdr_row)
        for i, h_text in enumerate(headers):
            cell = hdr_row.cells[i]
            if col_widths:
                cell.width = Inches(col_widths[i])
            set_cell_shading(cell, HEX_NAVY)
            c_top = 60 if compact else 120
            c_bot = 60 if compact else 120
            c_side = 100 if compact else 140
            f_size = Pt(8.5) if compact else Pt(9.5)
            f_dsize = Pt(8.0) if compact else Pt(9.0)

            set_cell_margins(cell, top=c_top, bottom=c_bot, left=c_side, right=c_side)
            set_cell_border(cell, 
                            top=('single', '6', HEX_NAVY),
                            bottom=('single', '12', HEX_TEAL),
                            left=('single', '4', '2C4B75'),
                            right=('single', '4', '2C4B75'))
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            p.paragraph_format.space_before = Pt(0)
            p.paragraph_format.space_after = Pt(0)
            run = p.add_run(h_text)
            run.bold = True
            run.font.size = f_size
            run.font.color.rgb = RGBColor(255, 255, 255)

        # Style Data Rows
        for r_idx, row_data in enumerate(data):
            row = tbl.rows[r_idx + 1]
            prevent_row_split(row)
            is_odd = (r_idx % 2 == 1)
            row_bg = HEX_ZEBRA if is_odd else "FFFFFF"
            for c_idx, cell_value in enumerate(row_data):
                cell = row.cells[c_idx]
                if col_widths:
                    cell.width = Inches(col_widths[c_idx])
                set_cell_shading(cell, row_bg)
                set_cell_margins(cell, top=c_top - 15, bottom=c_bot - 15, left=c_side, right=c_side)
                set_cell_border(cell, 
                                top=('single', '4', HEX_BORDER),
                                bottom=('single', '4', HEX_BORDER),
                                left=('none', '0', 'FFFFFF'),
                                right=('none', '0', 'FFFFFF'))
                p = cell.paragraphs[0]
                p.alignment = WD_ALIGN_PARAGRAPH.LEFT
                p.paragraph_format.space_before = Pt(0)
                p.paragraph_format.space_after = Pt(0)
                p.paragraph_format.line_spacing = 1.10 if compact else 1.15
                
                # Check for formatting in cell_value
                if isinstance(cell_value, tuple):
                    bold_lead, body_text = cell_value
                    rb = p.add_run(bold_lead)
                    rb.bold = True
                    rb.font.size = f_dsize
                    rb.font.color.rgb = COLOR_CHARCOAL
                    rt = p.add_run(body_text)
                    rt.font.size = f_dsize
                    rt.font.color.rgb = COLOR_CHARCOAL
                else:
                    r = p.add_run(str(cell_value))
                    r.font.size = f_dsize
                    r.font.color.rgb = COLOR_CHARCOAL

        p_space = doc.add_paragraph()
        p_space.paragraph_format.space_before = Pt(3)
        p_space.paragraph_format.space_after = Pt(3)
        return tbl

    # Helper: Add Bullet
    def add_bullet(doc, bold_prefix, text):
        p = doc.add_paragraph(style='List Bullet')
        p.paragraph_format.space_before = Pt(1)
        p.paragraph_format.space_after = Pt(3)
        p.paragraph_format.line_spacing = 1.15
        if bold_prefix:
            rb = p.add_run(bold_prefix)
            rb.bold = True
            rb.font.color.rgb = COLOR_CHARCOAL
        rt = p.add_run(text)
        rt.font.color.rgb = COLOR_CHARCOAL
        return p

    # Helper: Add Body Paragraph
    def add_body(doc, text, bold_prefix=None, space_after=4):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(space_after)
        p.paragraph_format.line_spacing = 1.15
        if bold_prefix:
            rb = p.add_run(bold_prefix)
            rb.bold = True
            rb.font.color.rgb = COLOR_CHARCOAL
        rt = p.add_run(text)
        rt.font.color.rgb = COLOR_CHARCOAL
        return p

    print("Framework initialized. Building Cover Page...")

    # ==========================================
    # 1. COVER PAGE (SECTION 1)
    # ==========================================
    sec1 = doc.sections[0]
    sec1.different_first_page_header_footer = True

    # Top spacing
    sec1.top_margin = Inches(0.75)
    sec1.bottom_margin = Inches(0.75)
    sec1.left_margin = Inches(0.85)
    sec1.right_margin = Inches(0.85)

    # Document Category Badge
    p_badge = doc.add_paragraph()
    p_badge.paragraph_format.space_before = Pt(0)
    p_badge.paragraph_format.space_after = Pt(2)
    r_badge = p_badge.add_run("ENTERPRISE TECHNICAL SPECIFICATION & PROJECT PROPOSAL")
    r_badge.font.name = 'Arial'
    r_badge.font.size = Pt(9)
    r_badge.font.bold = True
    r_badge.font.color.rgb = COLOR_TEAL

    # Document Main Title
    p_title = doc.add_paragraph()
    p_title.paragraph_format.space_before = Pt(2)
    p_title.paragraph_format.space_after = Pt(2)
    r_title = p_title.add_run("CareBasket: Scalable Healthcare & Digital Pharmacy E-Commerce Platform")
    r_title.font.name = 'Arial'
    r_title.font.size = Pt(21)
    r_title.font.bold = True
    r_title.font.color.rgb = COLOR_NAVY

    # Subtitle
    p_sub = doc.add_paragraph()
    p_sub.paragraph_format.space_before = Pt(1)
    p_sub.paragraph_format.space_after = Pt(8)
    r_sub = p_sub.add_run("A Secure, Responsive, and Regulatory-Compliant Healthcare Commerce Solution Tailored for the Bangladesh Market")
    r_sub.font.name = 'Arial'
    r_sub.font.size = Pt(11)
    r_sub.font.italic = True
    r_sub.font.color.rgb = COLOR_MUTED

    # Callout: Provisional Name & Originality
    add_callout(doc, "Provisional Project Notice & Scope Clarification", [
        "Provisional Project Codename: 'CareBasket' is designated as the temporary working title for this software engineering initiative. Final public branding, trademark registrations, and platform nomenclature remain fully configurable and subject to executive stakeholder determination prior to commercial launch.",
        "Originality & Non-Infringement Statement: While functionally inspired by the convenience and digital healthcare delivery models in Bangladesh (such as Arogga), CareBasket features an entirely bespoke technical architecture, original normalized database schema, custom responsive frontend design, and proprietary Clean Architecture implementation. No proprietary code, trade dress, or assets of any third party are utilized."
    ], kind="info")

    # Cover Page Metadata Table
    meta_headers = ["Project Specification Attribute", "Operational Proposal Details"]
    meta_data = [
        (("Project Provisional Title", ""), " CareBasket (Healthcare & Pharmacy E-Commerce)"),
        (("Document Type", ""), " Comprehensive Project Proposal & Technical Specification"),
        (("Target Geographic Market", ""), " Bangladesh (Dhaka Metropolitan & Nationwide Divisions)"),
        (("Target Currency & Locale", ""), " Bangladeshi Taka (BDT / ৳) | English (Initial) & Bengali (Phase 7)"),
        (("Core Architecture", ""), " Clean Architecture, ASP.NET Core Web API (.NET 10), EF Core"),
        (("Database Engine", ""), " PostgreSQL 16+ Relational Engine with Trigram Search & Strict Constraints"),
        (("Frontend Stack", ""), " Mobile-First Responsive HTML5, CSS3, JavaScript, Tailwind CSS"),
        (("Proposal Version", ""), " Version 1.0 (Formal Comprehensive Release)"),
        (("Preparation Date", ""), " October 2026"),
        (("Author & Role", ""), " Lead Software Solutions Architect & Senior Project Analyst"),
        (("Target Stakeholders", ""), " Project Sponsors, Executive Leadership, Development Team, Clinical Advisors"),
        (("Document Classification", ""), " Commercial in Confidence — Proprietary & Confidential")
    ]
    add_styled_table(doc, meta_headers, meta_data, col_widths=[2.4, 4.3], compact=True)

    # Executive Note at bottom of cover page
    p_cov_note = doc.add_paragraph()
    p_cov_note.paragraph_format.space_before = Pt(8)
    r_cn = p_cov_note.add_run("NOTICE: This proposal outlines the complete functional and technical scope, architectural blueprint, regulatory risk boundaries, and phased development lifecycle for the CareBasket platform. Implementation of production services must follow the staged phases detailed herein.")
    r_cn.font.size = Pt(8.5)
    r_cn.font.italic = True
    r_cn.font.color.rgb = COLOR_MUTED

    # ==========================================
    # SECTION BREAK -> MAIN BODY (SECTION 2)
    # ==========================================
    doc.add_page_break()
    sec2 = doc.add_section(docx.enum.section.WD_SECTION.NEW_PAGE)
    sec2.header.is_linked_to_previous = False
    sec2.footer.is_linked_to_previous = False

    # Header configuration
    hdr = sec2.header
    p_hdr = hdr.paragraphs[0]
    p_hdr.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    r_h1 = p_hdr.add_run("CareBasket — Healthcare E-Commerce Project Proposal  |  ")
    r_h1.font.size = Pt(8.5)
    r_h1.font.color.rgb = COLOR_MUTED
    r_h2 = p_hdr.add_run("CONFIDENTIAL")
    r_h2.font.size = Pt(8.5)
    r_h2.font.bold = True
    r_h2.font.color.rgb = COLOR_TEAL

    # Footer configuration
    ftr = sec2.footer
    p_ftr = ftr.paragraphs[0]
    p_ftr.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    r_f1 = p_ftr.add_run("CareBasket Technical Specification  |  Page ")
    r_f1.font.size = Pt(8.5)
    r_f1.font.color.rgb = COLOR_MUTED
    
    # Page field
    fld1 = parse_xml(r'<w:fldSimple %s w:instr="PAGE"/>' % nsdecls('w'))
    p_ftr._p.append(fld1)
    
    r_f2 = p_ftr.add_run(" of ")
    r_f2.font.size = Pt(8.5)
    r_f2.font.color.rgb = COLOR_MUTED
    
    fld2 = parse_xml(r'<w:fldSimple %s w:instr="NUMPAGES"/>' % nsdecls('w'))
    p_ftr._p.append(fld2)

    print("Cover page created. Generating Table of Contents...")

    # ==========================================
    # TABLE OF CONTENTS
    # ==========================================
    h_toc = doc.add_heading("Table of Contents", level=1)
    h_toc.paragraph_format.space_before = Pt(6)
    h_toc.paragraph_format.space_after = Pt(10)

    # Descriptive note about TOC
    add_body(doc, "The following outline provides direct navigational reference to all architectural, functional, security, and project governance sections of this proposal.")

    # High-fidelity visual Table of Contents Table
    toc_headers = ["Section", "Proposal Section Title", "Core Focus & Scope Coverage"]
    toc_data = [
        ("Section 1", "Executive Summary", "Strategic concept, target market, value proposition, and Release 1.0 scope."),
        ("Section 2", "Project Background & Problem Statement", "E-pharmacy landscape in Bangladesh, patient pain points, administrative challenges."),
        ("Section 3", "Project Objectives", "12 primary objectives, quantitative targets, and scope management principles."),
        ("Section 4", "Proposed System Overview & Architecture", "Clean Architecture pattern, multi-tier flow, data persistence, and embedded diagram."),
        ("Section 5", "Target Users and System Roles", "Customer, Admin, Pharmacist, Delivery, and AI Support roles with RBAC matrix."),
        ("Section 6", "Detailed Functional Feature List", "Exhaustive specifications for Modules A through I covering end-to-end commerce."),
        ("Section 7", "Proposed Technology Stack", "Frontend, ASP.NET Core backend, PostgreSQL, EF Core, Identity, and React rationale."),
        ("Section 8", "Payment and Delivery Strategy", "COD handling, bKash/Nagad roadmap, merchant onboarding realities, courier logistics."),
        ("Section 9", "Security, Privacy, and Compliance", "OWASP Top 10 hardening, prescription isolation, DGDA regulations, medical disclaimer."),
        ("Section 10", "Future AI Customer Support Service", "Conversational scope, RAG integration, clinical safety boundaries, triage escalation."),
        ("Section 11", "Project Development Phases", "Phases 1 through 7 breakdown, deliverables per stage, Release 1.0 vs future roadmap."),
        ("Section 12", "Testing Strategy & Quality Assurance", "Testing pyramid, overselling concurrency tests, disaster recovery, acceptance criteria."),
        ("Section 13", "Expected Deliverables", "Enumeration of all 14 project artifacts across code, docs, schemas, and test suites."),
        ("Section 14", "Assumptions, Constraints & Boundaries", "Market assumptions, payment dependencies, hosting constraints, out-of-scope boundaries."),
        ("Section 15", "Expected Outcomes & Success Criteria", "Business outcomes, quantitative technical KPIs, resilience, and operational metrics."),
        ("Section 16", "Project Governance & Risk Management", "Comprehensive risk matrix covering technical, regulatory, security, and financial risks."),
        ("Section 17", "Conclusion & Strategic Recommendations", "Final evaluation, architectural soundness, and immediate next steps.")
    ]
    add_styled_table(doc, toc_headers, toc_data, col_widths=[0.8, 2.5, 3.2], compact=True)

    doc.add_page_break()

    print("TOC generated. Writing Section 1: Executive Summary...")

    # ==========================================
    # SECTION 1: EXECUTIVE SUMMARY
    # ==========================================
    doc.add_heading("1. Executive Summary", level=1)
    
    add_body(doc, "The modern healthcare landscape in Bangladesh is experiencing a rapid digital transformation driven by expanding internet access, smartphone adoption, and a rising consumer expectation for verified, dependable healthcare deliveries. However, acquiring authentic medicines, navigating complex prescription requirements, and obtaining reliable pharmacy-related consumer goods continue to be challenging for millions of urban and rural citizens. Traditional retail pharmacy channels often struggle with inventory fragmentation, price opacity, physical congestion, and geographic disparities.")
    
    add_body(doc, "CareBasket (provisional working title) is conceived as an enterprise-grade digital healthcare e-commerce web application engineered specifically for the healthcare consumer market in Bangladesh. Inspired by the convenience, operational model, and delivery accessibility established by industry pioneers such as Arogga, CareBasket introduces a completely original platform identity, a bespoke relational data architecture, clean architectural separation of concerns, and an uncompromising commitment to clinical safety and data integrity.")

    add_body(doc, "The platform delivers a frictionless, multi-channel healthcare shopping experience allowing customers to:")
    add_bullet(doc, "Discover & Validate Healthcare Products: ", "Browse and search thousands of verified pharmaceutical items, OTC wellness products, medical devices, and personal hygiene supplies with transparent pricing and dosage details.")
    add_bullet(doc, "Fulfill Clinical Prescriptions: ", "Upload digital prescription documents (scans, camera captures, PDFs) through an encrypted, controlled-access channel for verification by licensed healthcare personnel prior to dispensing.")
    add_bullet(doc, "Complete Secure Checkouts: ", "Utilize a resilient multi-step checkout configured for the Bangladesh administrative hierarchy (8 Divisions, 64 Districts, Upazilas/Thanas), supporting Cash on Delivery (COD) initially and structured for immediate Mobile Financial Services (MFS) integration (bKash, Nagad).")
    add_bullet(doc, "Monitor End-to-End Fulfillment: ", "Track order lifecycles in real time from initial placement, through clinical prescription triage, warehouse fulfillment, dispatch, and final doorstep delivery.")

    add_body(doc, "Simultaneously, CareBasket equips pharmacy operators, clinical staff, and business administrators with a robust administrative backoffice suite. This suite encompasses centralized catalog management, real-time two-phase inventory reservation to prevent overselling, a dedicated Pharmacist Review Workbench, multi-role access control, financial audit logging, and actionable operational analytics.")

    add_callout(doc, "Release 1.0 Strategic Focus: Reliability, Integrity, and Compliance", [
        "Core Philosophy: The primary release of CareBasket intentionally prioritizes rock-solid reliability, atomic inventory integrity, responsive usability across low-bandwidth mobile networks, and clinical compliance over superficial feature bloat.",
        "Evolutionary Roadmap: High-complexity features—such as an automated AI-powered customer support bot, personalized machine-learning product recommendations, and automated third-party courier dispatch APIs—are designed modularly to be introduced in staged future releases following rigorous validation of core transactional workflows."
    ], kind="info")

    print("Writing Section 2: Project Background and Problem Statement...")

    # ==========================================
    # SECTION 2: PROJECT BACKGROUND AND PROBLEM STATEMENT
    # ==========================================
    doc.add_heading("2. Project Background & Problem Statement", level=1)

    doc.add_heading("2.1 The Healthcare Commerce Landscape in Bangladesh", level=2)
    add_body(doc, "Bangladesh represents one of the fastest-growing digital economies in South Asia, characterized by high mobile financial penetration and expanding e-commerce adoption. In the pharmaceutical sector, the domestic market produces over 98% of local medicinal demand, supported by well-regulated manufacturing standards. Nonetheless, the retail distribution tier remains heavily fragmented across tens of thousands of independent physical dispensaries, creating notable distribution bottlenecks, variable stock availability, and inconsistent customer service standards.")

    doc.add_heading("2.2 Critical Customer Pain Points in Existing Retail Channels", level=2)
    add_body(doc, "While several online pharmacy services operate in major metropolitan areas, significant segments of the population continue to face recurring challenges when purchasing medicines and health essentials:")
    
    add_bullet(doc, "Uncertain Product Availability and Stock Fragmentation: ", "Patients or their caregivers frequently visit multiple retail pharmacies to locate specific dosage forms, specialized chronic care medications, or pediatric syrups, incurring significant stress during medical emergencies.")
    add_bullet(doc, "Price Inconsistency and Information Asymmetry: ", "Customers frequently encounter opaque pricing, lack visibility into Maximum Retail Prices (MRP) set by the Directorate General of Drug Administration (DGDA), and have limited access to verified generic alternatives when branded medicines are unavailable.")
    add_bullet(doc, "Friction in Prescription Handling: ", "Physical submission of paper prescriptions is cumbersome, prone to document damage or loss, and creates queues at retail counters. Furthermore, few platforms offer transparent status updates regarding prescription validation.")
    add_bullet(doc, "Order Transparency and Fulfillment Deficits: ", "Standard e-commerce platforms often fail to provide granular tracking for sensitive healthcare orders, leaving patients uncertain whether vital medicines will arrive on time.")
    add_bullet(doc, "Payment and Logistics Inconvenience: ", "Many patients, particularly elderly citizens or individuals in suburban regions, require flexible payment options including Cash on Delivery and trusted local MFS channels (bKash/Nagad) tailored to regional addresses.")
    add_bullet(doc, "Customer Support Inaccessibility: ", "Traditional pharmacies lack responsive remote inquiry channels to answer basic questions regarding order status, product categories, or non-clinical dosage forms.")

    doc.add_heading("2.3 Administrative, Clinical, and Operational Challenges", level=2)
    add_body(doc, "On the retail and pharmacy management side, operators struggle with legacy, non-integrated operational practices:")
    add_bullet(doc, "Inventory Discrepancies and Overselling: ", "Without atomic, multi-phase inventory reservation systems, concurrent online orders routinely lead to stockouts, cancelled orders, and disgruntled customers.")
    add_bullet(doc, "Manual Prescription Review Bottlenecks: ", "Unstructured prescription triage processes lead to clinical backlogs, inconsistent record-keeping, and compliance liabilities regarding restricted or prescription-only medicines.")
    add_bullet(doc, "Logistics & Delivery Coordination Gaps: ", "Coordinating dispatching, delivery personnel assignments, and cash collection reconciliation across urban traffic without structured tooling results in delayed deliveries and revenue leakages.")

    add_callout(doc, "Balanced Market Perspective", [
        "It is recognized that well-established pharmacy chains and established digital leaders (such as Arogga) have demonstrated the viability and social value of e-pharmacy in Bangladesh. The CareBasket initiative does not claim that all existing services suffer from these deficiencies; rather, it identifies these persistent industry friction points as a clear opportunity to establish a next-generation platform engineered around superior reliability, architectural elegance, and clinical rigor."
    ], kind="info")

    print("Writing Section 3: Project Objectives...")

    # ==========================================
    # SECTION 3: PROJECT OBJECTIVES
    # ==========================================
    doc.add_heading("3. Project Objectives", level=1)
    
    add_body(doc, "The CareBasket initiative is governed by twelve foundational software engineering and business objectives. These objectives balance technical excellence, commercial practicality, and clinical responsibility:")

    obj_headers = ["No.", "Strategic Objective", "Technical Scope & Verification Target"]
    obj_data = [
        ("1", "Modern Healthcare E-Commerce Engine", "Architect and deploy an enterprise-grade digital storefront optimized for healthcare products, OTC wellness items, and prescription pharmaceuticals."),
        ("2", "Device-Agnostic Responsive Experience", "Deliver an ultra-fast, responsive web interface operating seamlessly across viewports from 320px mobile screens to 1280px+ desktop workstations."),
        ("3", "Secure Customer Identity & Profile Management", "Implement robust customer registration, authentication, session management, and address book governance using modern cryptographic security."),
        ("4", "Searchable Product Catalog with Trigram Fuzzy Search", "Provide sub-100ms catalog search supporting trade names, generic molecules, dosage forms, and categories with fuzzy typo tolerance."),
        ("5", "Shopping Cart, Wishlist & Guest Migration", "Support unauthenticated guest shopping carts that seamlessly merge into authenticated customer profiles upon login without item loss."),
        ("6", "Clinical Prescription Upload & Triage Workflow", "Build an encrypted prescription ingestion pipeline supporting multi-file uploads (.jpg, .png, .pdf) with formal pharmacist review gates."),
        ("7", "Atomic Inventory Reservation & Stock Protection", "Implement a two-phase reservation model (quantity on hand vs reserved) with database constraints to guarantee zero overselling under concurrency."),
        ("8", "Localized Bangladesh Payment Architecture", "Support Cash on Delivery (COD) and architect modular adapters for bKash and Nagad MFS payment processing with server-side validation."),
        ("9", "Real-Time Order Tracking & Notifications", "Provide transparent lifecycle tracking across 8 distinct order states, supported by automated transactional email and SMS notification hooks."),
        ("10", "Unified Administrative & Clinical Workbench", "Construct an intuitive backoffice portal enabling admins to govern products, inventory, users, financial reconciliations, and clinical reviews."),
        ("11", "Extensible Clean Architecture for Future Services", "Establish a decoupled ASP.NET Core backend enabling seamless future integration of AI customer support models and automated courier APIs."),
        ("12", "Rigorous Testing, Hardening & Quality Assurance", "Execute an automated testing pyramid encompassing unit tests, integration tests, concurrency simulations, and OWASP security audits.")
    ]
    add_styled_table(doc, obj_headers, obj_data, col_widths=[0.6, 2.4, 3.5])

    print("Writing Section 4: Proposed System Overview & Architecture...")

    # ==========================================
    # SECTION 4: PROPOSED SYSTEM OVERVIEW & ARCHITECTURE
    # ==========================================
    doc.add_heading("4. Proposed System Overview & Architecture", level=1)

    doc.add_heading("4.1 Architectural Philosophy: Clean Architecture", level=2)
    add_body(doc, "The CareBasket platform is structured according to the principles of Clean Architecture (Hexagonal / Onion Architecture). This design ensures that core domain entities, business logic, and clinical rules remain strictly decoupled from presentation frameworks, database engines, and third-party external services. This guarantees exceptional testability, maintainability, and long-term architectural longevity.")

    doc.add_heading("4.2 System Execution Flow", level=2)
    add_body(doc, "The high-level transaction flow of the CareBasket system operates across six coordinated tiers:")
    add_bullet(doc, "1. Client Interaction: ", "The user (customer, administrator, or pharmacist) interacts with the responsive frontend built with semantic HTML5, modern JavaScript, and Tailwind CSS.")
    add_bullet(doc, "2. API Gateway & Endpoints: ", "Client requests are transmitted securely over HTTPS as JSON payloads to the ASP.NET Core Web API layer. This layer enforces rate limiting, CORS policies, authentication middleware, and Swagger/OpenAPI documentation.")
    add_bullet(doc, "3. Application & Validation Core: ", "Incoming requests are validated via FluentValidation. Business logic handlers orchestrate use cases, enforce clinical rules, and check authorization policies.")
    add_bullet(doc, "4. Data Access & Persistence: ", "The application layer utilizes Entity Framework Core (EF Core) as an Object-Relational Mapper to translate domain operations into highly optimized SQL commands, executing within atomic database transactions.")
    add_bullet(doc, "5. Relational Database Engine: ", "PostgreSQL 16+ persists structured application data across 50+ relational tables, enforcing foreign keys, check constraints, unique indexes, and trigram text search extensions.")
    add_bullet(doc, "6. External Ecosystem Integrations: ", "Controlled infrastructure adapters communicate with external payment gateways (bKash/Nagad), SMS notification providers, private cloud document stores, and future AI support models.")

    doc.add_heading("4.3 High-Level System Architecture Diagram", level=2)
    add_body(doc, "The following architectural diagram illustrates the separation of concerns, communication protocols, and data flow across the presentation, backend, database, and third-party tiers:")

    # Insert Architecture Diagram Image
    if os.path.exists("architecture_diagram.png"):
        p_img = doc.add_paragraph()
        p_img.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img.paragraph_format.space_before = Pt(6)
        p_img.paragraph_format.space_after = Pt(4)
        run_img = p_img.add_run()
        run_img.add_picture("architecture_diagram.png", width=Inches(6.4))
        
        p_cap = doc.add_paragraph()
        p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_cap.paragraph_format.space_before = Pt(0)
        p_cap.paragraph_format.space_after = Pt(8)
        r_cap = p_cap.add_run("Figure 4.1: CareBasket High-Level Clean Architecture and System Interaction Diagram")
        r_cap.font.size = Pt(8.5)
        r_cap.font.italic = True
        r_cap.font.color.rgb = COLOR_MUTED
    else:
        add_body(doc, "[Architecture diagram image placeholder: Figure 4.1]")

    print("Writing Section 5: Target Users and System Roles...")

    # ==========================================
    # SECTION 5: TARGET USERS AND SYSTEM ROLES
    # ==========================================
    doc.add_heading("5. Target Users and System Roles", level=1)
    
    add_body(doc, "CareBasket employs a granular Role-Based Access Control (RBAC) framework supplemented by claims-based authorization policies. The platform identifies five primary actor categories, each with distinct responsibilities and system boundaries:")

    doc.add_heading("5.1 Role Personas and Responsibilities", level=2)
    
    add_bullet(doc, "1. Customer / Patient: ", "Registered end-users and unauthenticated guests who browse products, manage shopping carts and wishlists, save delivery addresses, submit digital prescriptions, place orders, choose payment methods, monitor fulfillment status, and submit post-purchase inquiries.")
    add_bullet(doc, "2. System Administrator: ", "Supervisory personnel responsible for platform configuration, catalog publishing, pricing and discount rules, inventory stock ledger audits, customer account management, role assignments, financial reconciliations, and reviewing system security audit logs.")
    add_bullet(doc, "3. Pharmacist / Authorized Prescription Reviewer: ", "Licensed healthcare professionals registered with the Pharmacy Council of Bangladesh or BMDC. They operate within a specialized Clinical Workbench to review prescription uploads, verify drug interactions and dosages, validate physician credentials, approve or reject prescription submissions, and annotate clinical orders.")
    add_bullet(doc, "4. Delivery Personnel / Logistics Partner: ", "Internal fleet couriers or third-party logistics agents with restricted, view-only mobile access to assigned delivery manifests, customer contact details, delivery address navigation, package hand-off confirmations, and Cash on Delivery collection logging.")
    add_bullet(doc, "5. Future AI Customer Support Service: ", "An automated software agent (planned for Phase 7) designed to answer operational questions, guide navigation, and check authenticated order statuses within strictly enforced clinical safety guardrails.")

    doc.add_heading("5.2 Role-Based Access Control (RBAC) Matrix", level=2)
    add_body(doc, "The following matrix details access privileges across core platform subsystems:")

    rbac_headers = ["Subsystem / Functional Domain", "Customer", "Pharmacist", "Admin", "Delivery Partner"]
    rbac_data = [
        ("Product Catalog & Search Browsing", "Full (Read)", "Full (Read)", "Full (Read/Write)", "Full (Read)"),
        ("Shopping Cart & Checkout Placement", "Full (Self)", "No Access", "Full (Impersonate/Admin)", "No Access"),
        ("Customer Profile & Saved Addresses", "Full (Self)", "View Only (In Order)", "Full (Read/Write)", "View Shipping Only"),
        ("Prescription Document Upload", "Upload (Self)", "No Access", "Upload (Assisted)", "No Access"),
        ("Prescription Clinical Verification Workbench", "View Own Status", "Full (Approve/Reject)", "View / Reassign", "No Access"),
        ("Inventory Stock Ledger & Low-Stock Alerts", "View In-Stock Flag", "View Stock Counts", "Full (Adjust/Ledger)", "No Access"),
        ("Order Fulfillment & Lifecycle Transitions", "Cancel (If Pending)", "Annotate Clinical", "Full State Override", "Update Delivered/Failed"),
        ("Payment Transaction Reconciliations", "View Own Invoices", "No Access", "Full (Reconcile/Refund)", "Record COD Collected"),
        ("Promotions, Coupons & Discounts", "Apply Valid Codes", "No Access", "Full (Create/Expire)", "No Access"),
        ("System Security & Audit Trail Logs", "No Access", "View Clinical Logs", "Full Security Access", "No Access"),
        ("Third-Party Courier Dispatch Configuration", "No Access", "No Access", "Full Configuration", "View Assigned Jobs"),
        ("AI Conversational Interface (Future)", "Full (Authenticated)", "Full (Testing)", "Manage Knowledge Base", "No Access")
    ]
    add_styled_table(doc, rbac_headers, rbac_data, col_widths=[2.5, 1.0, 1.0, 1.0, 1.0])

    print("Writing Section 6: Detailed Functional Requirements...")

    # ==========================================
    # SECTION 6: DETAILED FUNCTIONAL REQUIREMENTS AND FEATURE LIST
    # ==========================================
    doc.add_heading("6. Detailed Functional Requirements & Feature Specifications", level=1)
    add_body(doc, "The functional scope of CareBasket is decomposed into nine modular subsystems, ensuring clear separation of technical responsibilities and streamlined maintenance.")

    doc.add_heading("6.1 Module A: Customer-Facing Storefront & Browsing Experience", level=2)
    add_body(doc, "The storefront is the primary consumer entry point, engineered for lightning-fast responsiveness, high visual clarity, and low mobile bandwidth consumption:")
    add_bullet(doc, "Homepage & Dynamic Banners: ", "Curated product carousels (Featured Products, Seasonal Health, Chronic Care Essentials, Top Brands), promotional discount banners, emergency notice bars, and instant prescription upload quick-links.")
    add_bullet(doc, "Hierarchical Navigation: ", "Multi-level category hierarchy (Prescription Medicines, OTC Healthcare, Personal Care, Baby & Mother Care, Nutrition & Supplements, Medical Devices).")
    add_bullet(doc, "Trigram-Powered Smart Search: ", "Debounced search bar utilizing PostgreSQL pg_trgm fuzzy matching. Searches across brand names (e.g., 'Napa'), generic molecule names (e.g., 'Paracetamol'), manufacturers (e.g., 'Beximco'), and therapeutic classes.")
    add_bullet(doc, "Faceted Filtering & Sorting: ", "Filter by price range, brand, dosage form (Tablet, Syrup, Injection, Ointment, Capsule), prescription requirement flag, and pack size. Sort by popularity, price (low-to-high, high-to-low), and newest arrivals.")
    add_bullet(doc, "Product Detail Page (PDP): ", "High-resolution product imagery, verified pricing, pack quantity, active generic composition, manufacturer details, usage guidelines, safety precautions, storage instructions, and verified stock availability badge.")
    add_bullet(doc, "Cart & Wishlist Engine: ", "Persistent shopping cart storing item quantities, unit pricing, real-time stock checks, subtotal calculations, and one-click item removal. Integrated wishlist allowing customers to bookmark medications for future replenishment.")
    add_bullet(doc, "Localization Framework: ", "Clean architecture supporting English as the default launch language, with localization hooks structured for seamless Bengali (বাংলা) language toggling in Phase 7.")

    doc.add_heading("6.2 Module B: Product Catalog & Intelligent Inventory Management", level=2)
    add_body(doc, "The inventory system implements strict consistency guarantees to protect business operations against stockouts and overselling:")
    add_bullet(doc, "Catalog CRUD & Archival: ", "Comprehensive product onboarding workflows including title, slug, SKU, generic composition, category association, manufacturer, pack size, dosage form, and SEO metadata. Archival capabilities rather than hard-deletes preserve transactional history.")
    add_bullet(doc, "Two-Phase Atomic Inventory Reservation: ", "Inventory records maintain both quantity_on_hand and quantity_reserved. When a customer begins checkout, items are reserved atomically; upon payment or COD confirmation, reserved quantities are decremented. If an order expires or is cancelled, reservations are released immediately.")
    add_bullet(doc, "Batch & Expiry Date Tracking: ", "Support for inventory batches with batch numbers, manufacturing dates, and expiration dates to enforce First-Expiry-First-Out (FEFO) dispensing rules.")
    add_bullet(doc, "Low-Stock & Out-of-Stock Governance: ", "Automated administrative alerts triggered when stock levels fall below customizable thresholds. The catalog dynamically hides out-of-stock items or displays 'Out of Stock' badges, disabling purchase buttons.")
    add_bullet(doc, "Prescription-Only Drug (Rx) Flagging: ", "Every catalog item contains an explicit requires_prescription boolean. Any cart containing an Rx-flagged item mandates valid prescription verification before fulfillment.")

    doc.add_heading("6.3 Module C: Clinical Prescription Management & Verification", level=2)
    add_body(doc, "To uphold patient safety and comply with healthcare norms, CareBasket features a dedicated prescription management pipeline:")
    add_bullet(doc, "Secure Multi-File Upload: ", "Customers can upload prescription documents during checkout or via their account portal. Supports JPG, PNG, and PDF formats up to 10MB per file with mime-type validation and server-side virus scanning.")
    add_bullet(doc, "Encrypted Storage Isolation: ", "Prescription files are stored in an encrypted private cloud storage bucket or secure directory outside the web root, accessible only via time-limited, pre-signed URLs generated for authorized users.")
    add_bullet(doc, "Pharmacist Review Triage: ", "Prescriptions enter a Pending Review queue. Licensed pharmacists inspect the document, verify patient details, physician registration, date, and dosage instructions.")
    add_bullet(doc, "Structured Clinical Decisions: ", "Reviewers can Approve, Reject, or Request Clarification. Every decision records the reviewer ID, timestamp, and clinical notes in an immutable audit table.")
    add_bullet(doc, "Order Dispensing Lock: ", "Orders containing prescription-restricted items cannot progress to 'Processing' or 'Shipped' states until an authorized pharmacist attaches a formal clinical approval.")

    doc.add_heading("6.4 Module D: Resilient Order Processing & Lifecycle Governance", level=2)
    add_body(doc, "Order processing is orchestrated via a deterministic finite-state machine (FSM):")
    add_bullet(doc, "Finite Lifecycle States: ", "Pending -> Awaiting Prescription Review -> Confirmed -> Processing -> Out for Delivery -> Delivered -> Cancelled -> Refunded.")
    add_bullet(doc, "Customer Self-Cancellation: ", "Customers may cancel orders self-service only while in 'Pending' or 'Awaiting Prescription Review' states. Once an order enters 'Processing', cancellation requires administrative intervention.")
    add_bullet(doc, "Stock Ledger Reversal: ", "Any cancellation or item removal automatically triggers an inverse transaction in the stock movement ledger, returning reserved stock to active inventory.")
    add_bullet(doc, "Order History & Digital Invoicing: ", "Full customer order history with itemized tax/delivery breakdown, downloadable PDF invoices, and real-time tracking timelines.")

    doc.add_heading("6.5 Module E: Payment Management & Financial Architecture", level=2)
    add_body(doc, "Financial operations are designed for maximum resilience and transparency:")
    add_bullet(doc, "Cash on Delivery (COD): ", "Full support for COD orders, subject to regional delivery policies and maximum order value thresholds.")
    add_bullet(doc, "MFS Payment Gateway Integration (bKash & Nagad): ", "Modular payment abstraction layer engineered for bKash Tokenized Checkout and Nagad Payment Gateway. Features secure server-side payment execution, callback verification, and signature validation.")
    add_bullet(doc, "Idempotency & Duplicate Prevention: ", "All payment webhooks and capture callbacks require unique transaction reference IDs and idempotency tokens, preventing duplicate processing or double-charging.")
    add_bullet(doc, "Financial Audit Trail: ", "Every financial event (authorization, capture, settlement, refund) is logged with provider transaction IDs, timestamps, and amounts for accounting reconciliation.")

    doc.add_heading("6.6 Module F: Delivery Management & Bangladesh Geographic Hierarchy", level=2)
    add_body(doc, "Logistics management is optimized for the local infrastructure of Bangladesh:")
    add_bullet(doc, "Geographic Address Hierarchy: ", "Pre-seeded relational structure covering all 8 Administrative Divisions, 64 Districts, and 495+ Upazilas/Thanas, ensuring accurate address entry and eliminating free-form shipping errors.")
    add_bullet(doc, "Dynamic Delivery Fee Engine: ", "Configurable shipping fee calculation based on delivery zone (Inside Dhaka Metropolitan vs Outside Dhaka) and cart weight or value thresholds.")
    add_bullet(doc, "Delivery Manifests & Dispatch: ", "Administrative dispatch views group orders by delivery zone. Delivery personnel update package status (Dispatched, Delivered, Delivery Attempt Failed) with reason codes.")
    add_bullet(doc, "Future Courier API Interface: ", "Abstracted logistics service interfaces ready for third-party courier APIs (e.g., Pathao, RedX, Steadfast, eCourier) in Phase 7.")

    doc.add_heading("6.7 Module G: Multi-Channel Customer & Operational Notifications", level=2)
    add_body(doc, "Keeps customers and staff continuously informed across key transaction milestones:")
    add_bullet(doc, "Transactional Email: ", "Automated HTML emails for account verification, password reset, order confirmation, prescription status change, and delivery invoice dispatch.")
    add_bullet(doc, "SMS Notification Gateway: ", "Pluggable SMS interface configured for local Bangladeshi aggregators (e.g., SSL Wireless, BulkSMS BD) for OTP verification and time-critical delivery alerts.")
    add_bullet(doc, "Administrative Operational Alerts: ", "Real-time in-app dashboard badges for urgent pending prescriptions, low-stock alerts, and failed delivery notices.")

    doc.add_heading("6.8 Module H: Marketing, Promotions & Customer Engagement", level=2)
    add_body(doc, "Tools to foster customer retention and satisfaction:")
    add_bullet(doc, "Discount Codes & Coupons: ", "Coupon engine supporting percentage discounts, flat BDT deductions, minimum cart spending limits, maximum discount caps, usage limits per customer, and start/end validity dates.")
    add_bullet(doc, "Promotional Banners: ", "Admin-managed promotional sliders and contextual health advisory banners.")
    add_bullet(doc, "Customer Feedback & Reviews: ", "Post-delivery product rating and review submission with administrative moderation controls to prevent spam or inappropriate clinical claims.")

    doc.add_heading("6.9 Module I: Administrative Dashboard, Analytics & Audit Logging", level=2)
    add_body(doc, "Comprehensive operational intelligence for pharmacy executives and managers:")
    add_bullet(doc, "Operational KPI Dashboard: ", "Real-time metrics showing total daily orders, gross revenue, pending prescription count, low-stock warnings, and delivery fulfillment rates.")
    add_bullet(doc, "Sales & Inventory Reports: ", "Exportable CSV and PDF reports detailing revenue by category, best-selling medicines, stock valuation, and expired product audits.")
    add_bullet(doc, "Security Audit Logging: ", "Immutable audit log tracking all administrative actions, staff logins, prescription approval decisions, price updates, and permission modifications.")

    print("Writing Section 7: Proposed Technology Stack...")

    # ==========================================
    # SECTION 7: PROPOSED TECHNOLOGY STACK
    # ==========================================
    doc.add_heading("7. Proposed Technology Stack & Technical Justification", level=1)
    
    add_body(doc, "The technology stack for CareBasket is chosen based on enterprise reliability, type safety, sub-second execution speeds, and cost-effective hosting. The following table provides a comprehensive overview of each architectural layer:")

    tech_headers = ["Architectural Layer", "Proposed Technology", "Version / Standard", "Strategic Justification"]
    tech_data = [
        ("Frontend Presentation", "HTML5, CSS3, Modern JavaScript", "ES6+ Standards", "High speed, zero bloated framework runtime, universal mobile browser compatibility across 2G/3G/4G."),
        ("CSS Framework", "Tailwind CSS", "v3.4+ / v4.0", "Utility-first design, microscopic purged CSS bundle sizes (<40KB), rapid responsive UI implementation."),
        ("Optional Frontend SPA", "React", "v18+ (Deferred / Optional)", "Optional future SPA enhancement for complex interactive modules; not required for initial release."),
        ("Backend Web API", "ASP.NET Core Web API", ".NET 10 / .NET 8 LTS", "Industry-leading throughput, cross-platform performance, strong typing, clean architecture support."),
        ("Object-Relational Mapper", "Entity Framework Core (EF Core)", "EF Core 10 / 8", "High-performance LINQ data querying, automatic SQL optimization, automated database migrations."),
        ("Database Engine", "PostgreSQL", "Version 16+", "Robust ACID relational guarantees, check constraints, JSONB flexibility, pg_trgm fuzzy search index support."),
        ("API Communication", "RESTful HTTP APIs", "JSON Payload / HTTPS", "Standardized, stateless, lightweight client-server communication with standard HTTP verbs and status codes."),
        ("Authentication & Security", "ASP.NET Core Identity & JWT", "Bearer Tokens / PBKDF2", "Secure token-based authentication, claims-based role guards, salted cryptographic password hashing."),
        ("Authorization Mechanism", "RBAC & Policy-Based Auth", "ASP.NET Core Policies", "Least-privilege security model segregating Customer, Pharmacist, Admin, and Courier permissions."),
        ("API Documentation", "Swagger / OpenAPI", "OpenAPI 3.0 Standard", "Interactive living documentation for rapid developer onboarding, contract verification, and testing."),
        ("Automated Testing Suite", "xUnit, FluentAssertions, Playwright", "xUnit 2.8+ / Playwright", "Complete testing pyramid covering unit business logic, API integration tests, and cross-browser E2E."),
        ("Version Control & CI/CD", "Git & GitHub", "GitHub Actions", "Distributed source control, branch protection rules, automated CI pipeline for builds, linting, and tests."),
        ("Hosting & Deployment", "Linux Cloud VPS / Docker Containers", "Ubuntu 24.04 LTS / Alpine", "Cost-effective, highly scalable production hosting behind an NGINX reverse proxy with automated SSL.")
    ]
    add_styled_table(doc, tech_headers, tech_data, col_widths=[1.5, 1.8, 1.2, 2.0])

    doc.add_heading("7.1 Frontend Architecture & The Justification for Vanilla JS / Tailwind", level=2)
    add_body(doc, "A critical architectural decision for the CareBasket initial release is utilizing modular HTML5, modern JavaScript, and Tailwind CSS rather than mandating a heavy JavaScript Single Page Application (SPA) framework such as React or Angular:")
    add_bullet(doc, "Mobile Network Performance: ", "Healthcare e-commerce in Bangladesh serves users across diverse mobile devices and variable mobile networks (3G/4G). Heavy JavaScript bundles can introduce 3 to 6-second Time-to-Interactive (TTI) delays on mid-range smartphones. Modular JavaScript with purged Tailwind CSS yields sub-second initial paint times.")
    add_bullet(doc, "SEO & Discoverability: ", "Medicinal products, health advice, and wellness articles benefit substantially from fast server-rendered or pre-rendered HTML, ensuring superior indexing on search engines like Google without complex Server-Side Rendering (SSR) overhead.")
    add_bullet(doc, "Optional React Role Clarification: ", "React is formally categorized as an optional frontend enhancement. It is retained on the technology roadmap for potential future integration in specialized, state-intensive portals (such as a high-frequency Pharmacist Review Interactive Canvas or Real-time Warehouse Picking Tool) if business needs justify it. For the public-facing storefront and general admin panels, the lightweight Tailwind architecture remains superior for Release 1.0.")

    doc.add_heading("7.2 Backend & Database Architecture Rationale", level=2)
    add_body(doc, "ASP.NET Core coupled with PostgreSQL provides an exceptionally resilient foundation:")
    add_bullet(doc, "Enterprise Throughput & Memory Efficiency: ", "ASP.NET Core consistently ranks among the fastest web application frameworks in independent benchmarks (TechEmpower), processing tens of thousands of requests per second with minimal CPU and memory footprints.")
    add_bullet(doc, "Relational Data Integrity in Healthcare: ", "Healthcare e-commerce mandates absolute transactional consistency. Financial orders, prescription attachments, and inventory levels cannot tolerate the eventual consistency anomalies common in NoSQL databases. PostgreSQL 16+ enforces strict foreign key constraints, check constraints, composite unique indexes, and ACID transaction isolation.")
    add_bullet(doc, "Bilingual Search Support (pg_trgm): ", "PostgreSQL's native pg_trgm (trigram) extension allows high-performance fuzzy text matching, enabling customers to locate drugs even when their search query contains minor spelling errors or phonetic transliterations.")

    print("Writing Section 8: Payment and Delivery Strategy...")

    # ==========================================
    # SECTION 8: PAYMENT AND DELIVERY STRATEGY
    # ==========================================
    doc.add_heading("8. Payment and Delivery Strategy for Bangladesh", level=1)

    doc.add_heading("8.1 Payment Landscape: Cash on Delivery & Mobile Financial Services", level=2)
    add_body(doc, "Consumer spending in Bangladesh's digital economy is characterized by a dual reliance on Cash on Delivery (COD) for risk-averse shoppers and Mobile Financial Services (MFS)—primarily bKash and Nagad—for digital settlements:")
    add_bullet(doc, "Cash on Delivery (COD) Strategy: ", "COD will serve as the default payment option for the initial release. COD lowers barriers to adoption for first-time healthcare shoppers. CareBasket protects against non-collection risks by implementing automated order confirmation SMS alerts, customer phone verification, and administrative order value caps for unverified customer accounts.")
    add_bullet(doc, "MFS Digital Payments (bKash & Nagad): ", "Integrating bKash and Nagad provides instantaneous checkout, pre-payment guarantees, and reduced cash-handling liabilities for delivery couriers.")

    add_callout(doc, "Production Payment Gateway Realities vs. Sandbox Demos", [
        "A critical project governance principle of CareBasket is acknowledging that production payment processing requires extensive formal commercial onboarding. A functional sandbox integration is merely an engineering validation and does NOT constitute a live payment system.",
        "Production Readiness Mandates:",
        "1. Formal Merchant Onboarding: Executing legal agreements with bKash (bKash Merchant Account) and Nagad (Nagad Payment Gateway) requiring Trade License, TIN Certificate, BIN/VAT Registration, and corporate bank accounts.",
        "2. Production API Credentials: Secure provisioning of production app keys, app secrets, merchant usernames, and private signing keys.",
        "3. Secure Server-Side Webhook Verification: All payment authorizations must be verified directly via server-to-server cryptographic callbacks. Frontend payment confirmations must never be trusted.",
        "4. Automated Reconciliation & Settlement: Establishing automated ledger reconciliation to track settlement cycles, provider transaction fees (1.5% - 2.0%), and dispute resolution procedures."
    ], kind="warning")

    doc.add_heading("8.2 Delivery Strategy: Urban Speed & Regional Reach", level=2)
    add_body(doc, "Delivery logistics represent the operational backbone of healthcare fulfillment. CareBasket implements a two-tier delivery model:")
    add_bullet(doc, "Intra-City Dhaka Express Delivery: ", "Within the Dhaka metropolitan area, orders are routed to dedicated regional fulfillment hubs, enabling same-day or next-day delivery for emergency and urgent wellness supplies.")
    add_bullet(doc, "Nationwide Division & District Logistics: ", "For locations outside Dhaka across the 64 districts, orders are consolidated and dispatched via established domestic logistics couriers (e.g., Steadfast Courier, RedX, Pathao, eCourier), typically delivering within 48 to 72 hours.")
    add_bullet(doc, "Cold-Chain & Temperature Consideration: ", "Catalog flags identify temperature-sensitive medications (e.g., Insulins, Vaccines, Biologics). Release 1.0 limits the sale of cold-chain items to localized express delivery zones equipped with insulated packaging.")

    print("Writing Section 9: Security, Privacy, and Compliance...")

    # ==========================================
    # SECTION 9: SECURITY, PRIVACY, AND REGULATORY COMPLIANCE
    # ==========================================
    doc.add_heading("9. Security, Privacy, and Regulatory Compliance", level=1)

    doc.add_heading("9.1 Enterprise Security Architecture & Vulnerability Defense", level=2)
    add_body(doc, "CareBasket processes sensitive personal health data, necessitating defense-in-depth security controls aligned with OWASP Top 10 guidelines:")
    add_bullet(doc, "Strict Server-Side Validation: ", "Every incoming API request is scrutinized via FluentValidation. Parameterized SQL queries within EF Core completely eliminate SQL Injection (SQLi) risks.")
    add_bullet(doc, "Cross-Site Scripting (XSS) & Content Security Policy: ", "Strict output encoding, input sanitization, and Content Security Policy (CSP) HTTP headers prevent stored or reflected script injection.")
    add_bullet(doc, "Cryptographic Password Storage: ", "User credentials are protected using ASP.NET Core Identity's implementation of PBKDF2 with HMAC-SHA256 and unique per-user cryptographic salts.")
    add_bullet(doc, "Transport Layer Security (HTTPS): ", "Strict-Transport-Security (HSTS) headers enforce TLS 1.3 encryption across all client-server communications in production.")
    add_bullet(doc, "Secrets Management: ", "Database connection strings, JWT signing keys, and external gateway tokens are managed strictly via environment variables and secret stores, never committed to source control.")
    add_bullet(doc, "Rate Limiting & Anti-Abuse: ", "ASP.NET Core rate limiting middleware throttles authentication endpoints, OTP generation requests, and search queries, mitigating credential stuffing and denial-of-service (DoS) attempts.")

    doc.add_heading("9.2 Privacy Protection for Prescription Documents & Medical Records", level=2)
    add_body(doc, "Prescriptions constitute sensitive Personal Health Information (PHI) requiring enhanced data protections:")
    add_bullet(doc, "Controlled Storage Isolation: ", "Prescription files are stored in an isolated, private file repository outside public HTTP roots. Direct URL file enumeration is impossible.")
    add_bullet(doc, "Time-Limited Pre-Signed URLs: ", "Access to prescription images is granted exclusively through short-lived (e.g., 5-minute validity) cryptographically signed tokens issued only to authenticated pharmacists or the owning customer.")
    add_bullet(doc, "Retention and Deletion Policies: ", "Configurable automated data lifecycle policies govern the archival and secure purging of prescription images following mandatory clinical record retention periods.")

    doc.add_heading("9.3 Regulatory Context in Bangladesh & Critical Healthcare Disclaimers", level=2)
    add_body(doc, "Digital healthcare platforms operate within a dynamic regulatory landscape governed by several key statutes in Bangladesh:")
    add_bullet(doc, "The Drugs Act, 1940 & Drugs Rules, 1946: ", "Governing the import, manufacture, distribution, and sale of allopathic medicines.")
    add_bullet(doc, "National Drug Policy of Bangladesh (2016): ", "Mandating quality control, pricing transparency, and the prevention of irrational antibiotic dispensing.")
    add_bullet(doc, "Cyber Security Act / ICT Act: ", "Governing electronic data processing, customer privacy, and the legal recognition of digital commercial transactions.")
    add_bullet(doc, "Directorate General of Drug Administration (DGDA): ", "Regulatory oversight regarding registered pharmaceutical pricing, scheduled narcotics, and restricted prescription dispensing.")

    add_callout(doc, "Mandatory Health Service Disclaimer & Ethical Scope", [
        "1. Commerce Platform, Not a Clinic: CareBasket is strictly an e-commerce fulfillment and distribution platform for legitimate pharmaceutical and wellness products. It is NOT a medical clinic, telemedicine provider, or substitute for professional medical diagnosis or clinical consultation.",
        "2. Clinical Consultation Primacy: Customers are consistently advised to consult registered medical practitioners before taking any new medication. Emergency medical situations must be directed immediately to physical emergency rooms or national emergency services (999).",
        "3. Formal Compliance Disclaimer: Prior to production commercial launch, CareBasket's operational workflows and physical pharmacy distribution facilities must undergo formal legal audit and licensing review by certified legal counsel and regulatory specialists in Bangladesh."
    ], kind="medical")

    print("Writing Section 10: Future AI Customer Support Service...")

    # ==========================================
    # SECTION 10: FUTURE AI CUSTOMER SUPPORT SERVICE
    # ==========================================
    doc.add_heading("10. Future AI-Powered Customer Support Service", level=1)

    doc.add_heading("10.1 Planned Conversational Capabilities (Phase 7 Roadmap)", level=2)
    add_body(doc, "In Phase 7, CareBasket plans to introduce an advanced conversational AI assistant designed to provide 24/7 first-line customer support. Operating via modern Large Language Models (LLMs) enhanced by Retrieval-Augmented Generation (RAG), the support service will deliver:")
    add_bullet(doc, "Platform Navigation & Onboarding: ", "Guiding users on how to register, upload prescriptions, redeem coupon codes, and select delivery zones.")
    add_bullet(doc, "Bilingual Assistance: ", "Conducting seamless, natural-language dialogues in both English and colloquial Bengali (বাংলা).")
    add_bullet(doc, "Authenticated Order Status Lookup: ", "Safely retrieving shipping status and delivery ETAs for logged-in, verified users through secure API function-calling.")
    add_bullet(doc, "Vetted Product Information Retrieval: ", "Answering factual questions regarding OTC health products, pack sizes, and manufacturer guidelines sourced strictly from approved DGDA package inserts.")

    doc.add_heading("10.2 Strict Clinical Safety Boundaries & Operational Guardrails", level=2)
    add_body(doc, "To protect human life and eliminate legal liabilities, the AI service will be governed by rigid, non-negotiable safety guardrails enforced through deterministic prompt constraints and secondary moderation layers:")

    ai_headers = ["Prohibited AI Behavior", "Enforced Safety Boundary & System Reaction"]
    ai_data = [
        ("Medical Diagnosis", "The AI is strictly forbidden from diagnosing diseases or evaluating patient symptoms. It will state: 'I am an automated assistant, not a doctor. Please consult a registered physician.'"),
        ("Prescription Recommendation", "The AI will never recommend prescription-only medications or alter physician-prescribed dosage schedules."),
        ("Dispensing Override", "The AI possesses zero authority to approve prescriptions or bypass the mandatory pharmacist verification gate."),
        ("Hallucinated Information", "The AI will never invent stock availability, pricing, or delivery dates. All factual data must be grounded in verified database API responses."),
        ("Emergency Triage Detection", "Queries mentioning chest pain, acute bleeding, poisoning, or severe trauma immediately trigger urgent care instructions and emergency hotlines (999)."),
        ("Human Escalation Handover", "Any complex, unresolved, or emotionally charged inquiry is immediately transferred to a human customer support ticket queue.")
    ]
    add_styled_table(doc, ai_headers, ai_data, col_widths=[2.5, 4.0])

    print("Writing Section 11: Development Phases...")

    # ==========================================
    # SECTION 11: PROJECT DEVELOPMENT PHASES
    # ==========================================
    doc.add_heading("11. Project Development Phases & Implementation Timeline", level=1)
    
    add_body(doc, "CareBasket adopts an Agile, phase-gate engineering methodology divided into seven distinct phases. Phases 1 through 6 encompass the core scope required for the Initial Release (Release 1.0), while Phase 7 encompasses post-launch evolutionary enhancements:")

    phase_headers = ["Phase", "Phase Name", "Primary Engineering Focus & Deliverables", "Target Milestone"]
    phase_data = [
        ("Phase 1", "Requirements & Planning", "System requirements specification, user story mapping, DB entity modeling, UI wireframes, security baseline.", "Milestone 1: Approved Architecture"),
        ("Phase 2", "Database & Backend API", "PostgreSQL schema, EF Core migrations, Clean Architecture setup, JWT Identity, catalog & inventory APIs.", "Milestone 2: Functional Core API"),
        ("Phase 3", "Frontend Storefront & Admin", "Responsive HTML5/Tailwind UI, catalog views, cart, checkout UI, customer account, admin backoffice portal.", "Milestone 3: Complete UI Suite"),
        ("Phase 4", "Core Workflow Integration", "Connecting frontend to APIs, prescription upload pipeline, COD order lifecycle, notification triggers.", "Milestone 4: End-to-End System"),
        ("Phase 5", "Testing & Quality Assurance", "Unit test suite, concurrency overselling tests, Playwright responsive audit (320px-1280px), security audit.", "Milestone 5: Production-Ready QA"),
        ("Phase 6", "Staging & Production Release", "Dockerization, NGINX reverse proxy, HTTPS certs, DB backup automation, staff training, Pilot Launch.", "Milestone 6: Release 1.0 Live"),
        ("Phase 7", "Post-Launch Evolution", "AI customer support bot, production bKash/Nagad APIs, courier automated dispatch, Bengali localization.", "Milestone 7: Advanced Platform")
    ]
    add_styled_table(doc, phase_headers, phase_data, col_widths=[0.8, 1.8, 2.8, 1.6])

    print("Writing Section 12: Testing Strategy...")

    # ==========================================
    # SECTION 12: TESTING STRATEGY AND QUALITY ASSURANCE
    # ==========================================
    doc.add_heading("12. Comprehensive Testing Strategy & Quality Assurance", level=1)
    add_body(doc, "Quality engineering is central to CareBasket's architectural philosophy. Testing follows a comprehensive multi-tier testing pyramid to guarantee stability, security, and performance:")

    doc.add_heading("12.1 Testing Pyramid Methodology", level=2)
    add_bullet(doc, "1. Unit Testing (xUnit & Moq): ", "Testing isolated domain business logic, coupon calculation rules, delivery fee logic, prescription status transitions, and data validation rules with high code coverage.")
    add_bullet(doc, "2. API & Integration Testing (WebApplicationFactory): ", "Executing automated HTTP requests against in-memory PostgreSQL test containers to verify API endpoints, database constraint enforcement, and transaction rollbacks.")
    add_bullet(doc, "3. Concurrency & Overselling Stress Tests: ", "Simulating dozens of simultaneous checkout requests for the final remaining inventory item to guarantee that the two-phase reservation engine prevents overselling.")
    add_bullet(doc, "4. Security & Vulnerability Auditing: ", "Automated and manual penetration testing covering JWT token tampering, privilege escalation attempts, SQLi probes, XSS injection, and malicious file upload detection.")
    add_bullet(doc, "5. Cross-Device Responsive UI Testing (Playwright): ", "Automated visual and functional testing across target viewports: 320px (compact mobile), 375px (standard smartphone), 768px (tablet), and 1280px+ (desktop).")
    add_bullet(doc, "6. Disaster Recovery & Backup Restoration Drills: ", "Validating that automated PostgreSQL pg_dump database snapshots can be fully restored to an operational standby instance within 15 minutes.")

    doc.add_heading("12.2 Formal Sample Acceptance Criteria", level=2)
    add_body(doc, "The following acceptance criteria define production readiness for core transactional workflows:")

    ac_headers = ["Functional Scope", "Test Scenario", "Expected Acceptance Criteria", "Verification Tool"]
    ac_data = [
        ("User Authentication", "Customer registration and login", "Valid registration sends verification email; login returns secure HTTP-only JWT; invalid passwords throttled.", "xUnit / Integration Test"),
        ("Catalog Search", "Fuzzy search for misspelled drugs", "Searching 'Paracitamol' successfully returns 'Paracetamol' formulations within 100ms.", "API Integration Test"),
        ("Inventory Protection", "Concurrent checkout on last unit", "Out of 10 concurrent requests, exactly 1 order succeeds; 9 receive out-of-stock notification; stock does not go negative.", "Parallel Stress Test"),
        ("Clinical Prescription Gate", "Order containing Rx medicine", "Order status set to 'Awaiting Prescription Review'; processing blocked until licensed pharmacist clicks 'Approve'.", "End-to-End Workflow"),
        ("Access Control", "Customer accessing Admin endpoints", "API returns HTTP 403 Forbidden; attempt logged to administrative security audit trail.", "Security Auth Test"),
        ("Mobile Responsiveness", "Checkout flow on 320px mobile", "No horizontal scrollbars; form inputs and checkout buttons easily clickable (min 44px touch targets).", "Playwright Visual Audit"),
        ("Payment Verification", "Simulated bKash callback", "Backend validates transaction signature directly with provider API; duplicate callbacks return HTTP 200 without double-crediting.", "Webhook Test Suite")
    ]
    add_styled_table(doc, ac_headers, ac_data, col_widths=[1.4, 1.6, 2.4, 1.6])

    print("Writing Section 13: Expected Deliverables...")

    # ==========================================
    # SECTION 13: EXPECTED DELIVERABLES
    # ==========================================
    doc.add_heading("13. Expected Deliverables", level=1)
    
    add_body(doc, "Upon completion of the development lifecycle, the CareBasket engineering project will produce fourteen tangible, verifiable deliverables:")

    deliv_headers = ["No.", "Project Deliverable", "Format / Repository", "Target Recipient / Description"]
    deliv_data = [
        ("1", "Complete Source Code Repository", "Git / GitHub Repo", "Full, production-ready codebase structured under Clean Architecture with branch history."),
        ("2", "Responsive Customer Web Storefront", "Web Application", "Mobile-first responsive frontend optimized for discovery, cart, checkout, and tracking."),
        ("3", "Administrative & Pharmacist Workbench", "Web Application", "Role-guarded backoffice for catalog, inventory, clinical triage, and analytics."),
        ("4", "ASP.NET Core Web API Backend", "Compiled .NET Service", "High-performance REST API services exposing all endpoints and business rules."),
        ("5", "PostgreSQL Database Schema & Migrations", "SQL / EF Core Migrations", "50+ relational tables with foreign keys, check constraints, and seeding scripts."),
        ("6", "Environment Configuration Templates", ".env.example & json", "Sanitized environment templates with documented variables, free of hardcoded secrets."),
        ("7", "Interactive API Documentation", "OpenAPI / Swagger UI", "Living interactive documentation covering all request/response models and auth schemes."),
        ("8", "Automated Test Suite & Test Report", "xUnit / Playwright Logs", "Comprehensive unit, integration, and E2E test suites with passing execution reports."),
        ("9", "System Setup & Installation Manual", "Markdown / docs/", "Step-by-step developer guide for local environment configuration and seeding."),
        ("10", "Production Deployment & Hosting Guide", "Markdown / docs/", "Infrastructure manual for NGINX, Docker, SSL certificates, and cloud VPS provisioning."),
        ("11", "Database Backup & Disaster Recovery Guide", "Markdown / docs/", "Operational runbook for automated backups, retention, and emergency restoration."),
        ("12", "System Architecture & ER Diagrams", "Image / PlantUML / Markdown", "High-resolution architectural flowcharts and entity-relationship diagrams."),
        ("13", "User Manuals & Operational SOPs", "PDF / Word Document", "Comprehensive user guides for customers, administrators, and reviewing pharmacists."),
        ("14", "Future Evolution & AI Integration Roadmap", "PDF / Strategic Plan", "Architectural blueprint for Phase 7 AI support, live MFS, and courier API integrations.")
    ]
    add_styled_table(doc, deliv_headers, deliv_data, col_widths=[0.6, 2.3, 1.6, 2.5])

    print("Writing Section 14: Assumptions, Constraints and Scope Boundaries...")

    # ==========================================
    # SECTION 14: ASSUMPTIONS, CONSTRAINTS, AND SCOPE BOUNDARIES
    # ==========================================
    doc.add_heading("14. Assumptions, Constraints, and Scope Boundaries", level=1)

    doc.add_heading("14.1 Key Project Assumptions", level=2)
    add_bullet(doc, "Geographic Focus: ", "The platform is engineered exclusively for the Bangladesh healthcare retail market, operating in Bangladeshi Taka (BDT / ৳).")
    add_bullet(doc, "Language Phasing: ", "English serves as the initial platform language for Release 1.0; complete Bengali (বাংলা) localization is scheduled for Phase 7.")
    add_bullet(doc, "Catalog Source Accuracy: ", "Initial pharmaceutical catalog data, generic molecule mappings, and manufacturer details will be provided or verified by licensed pharmacists using official DGDA publications.")
    add_bullet(doc, "Legal & Merchant Prerequisites: ", "Live digital payment integrations (bKash/Nagad) assume the project sponsors possess the necessary corporate documentation (Trade License, TIN, Bank Merchant Accounts).")

    doc.add_heading("14.2 Scope Boundaries: Initial Release 1.0 vs. Out-of-Scope Roadmap", level=2)
    add_body(doc, "Clear scope boundaries ensure predictable project delivery without budget or timeline overruns:")

    scope_headers = ["Functional Domain", "In-Scope for Initial Release (1.0)", "Out-of-Scope (Deferred to Phase 7+)"]
    scope_data = [
        ("Customer Storefront", "Full catalog browsing, fuzzy search, cart, checkout, profile.", "Native iOS/Android mobile apps (PWA supported)."),
        ("Prescription Workflow", "Multi-file upload, secure storage, pharmacist approve/reject.", "Automated OCR prescription handwriting transcription."),
        ("Payment Processing", "Cash on Delivery (COD) + MFS API integration readiness.", "Live merchant settlement contracts (requires legal entity)."),
        ("Customer Support", "Standard FAQ, email support, and contact form ticketing.", "Autonomous conversational AI chatbot."),
        ("Logistics & Shipping", "Division-District-Upazila hierarchy, manual dispatch routing.", "Automated real-time 3rd-party courier API auto-dispatch."),
        ("Marketing & Loyalty", "Promotional coupon codes, banners, and flat discounts.", "Complex gamified loyalty reward points system."),
        ("Clinical Scope", "Dispensing governance and prescription document checks.", "Telemedicine consultations or direct doctor prescribing.")
    ]
    add_styled_table(doc, scope_headers, scope_data, col_widths=[1.5, 2.6, 2.9])

    print("Writing Section 15: Expected Outcomes and Success Criteria...")

    # ==========================================
    # SECTION 15: EXPECTED OUTCOMES AND SUCCESS CRITERIA
    # ==========================================
    doc.add_heading("15. Expected Outcomes and Success Criteria", level=1)
    
    add_body(doc, "The successful deployment of CareBasket will yield substantial operational and commercial advantages while satisfying rigorous technical benchmarks:")

    doc.add_heading("15.1 Commercial and Operational Outcomes", level=2)
    add_bullet(doc, "Expanded Healthcare Access: ", "Customers across Bangladesh gain reliable access to verified medicines delivered directly to their doorsteps.")
    add_bullet(doc, "Elimination of Inventory Shrinkage: ", "Atomic two-phase reservation eradicates overselling and stockout-driven order cancellations.")
    add_bullet(doc, "Enhanced Clinical Governance: ", "Structured pharmacist triage establishes full transparency and accountability for prescription dispensing.")
    add_bullet(doc, "Streamlined Operational Efficiency: ", "Centralized administrative dashboards drastically reduce manual phone order taking and dispatch coordination overhead.")

    doc.add_heading("15.2 Quantitative Technical Success Criteria", level=2)
    add_body(doc, "Release 1.0 must satisfy the following verifiable performance and quality thresholds before receiving production deployment sign-off:")

    kpi_headers = ["Performance / Quality Metric", "Target Benchmark", "Verification Method"]
    kpi_data = [
        ("Mobile Page Load Speed", "< 1.5 seconds on 4G networks (First Contentful Paint)", "Google Lighthouse / WebPageTest"),
        ("Database Catalog Search Latency", "< 100 ms execution time for trigram fuzzy queries", "PostgreSQL EXPLAIN ANALYZE"),
        ("Concurrent Overselling Tolerance", "0% inventory variance under 50 simultaneous checkouts", "Parallel Load Testing (k6 / Artillery)"),
        ("Automated Test Suite Pass Rate", "100% pass rate across all unit and integration tests", "GitHub Actions CI Pipeline"),
        ("Critical Security Defect Threshold", "Zero unresolved High or Critical vulnerabilities", "OWASP ZAP / Dependency Vulnerability Scan"),
        ("Mobile Responsiveness Compliance", "Zero horizontal overflow across 320px, 375px, 768px viewports", "Playwright Multi-Viewport Test Suite"),
        ("Disaster Recovery RTO & RPO", "RTO < 15 minutes; RPO < 1 hour for automated DB snapshots", "Staging Server Disaster Recovery Drill")
    ]
    add_styled_table(doc, kpi_headers, kpi_data, col_widths=[2.4, 2.4, 2.2])

    print("Writing Section 16: Project Governance and Risk Management...")

    # ==========================================
    # SECTION 16: PROJECT GOVERNANCE AND RISK MANAGEMENT
    # ==========================================
    doc.add_heading("16. Project Governance & Risk Management", level=1)
    add_body(doc, "Proactive risk management ensures that potential technical, regulatory, and operational obstacles are identified early and mitigated effectively:")

    risk_headers = ["Risk Domain & Description", "Impact", "Likelihood", "Proactive Mitigation Strategy"]
    risk_data = [
        ("Regulatory Scrutiny on E-Pharmacies: Changes in DGDA digital dispensing policies.", "High", "Medium", "Strict prescription verification gates; pharmacist review logging; consultation with specialized healthcare legal counsel."),
        ("MFS Gateway Onboarding Delays: Bureaucratic delays in acquiring live bKash/Nagad merchant IDs.", "Medium", "High", "Deploy with robust Cash on Delivery (COD) workflows; modular payment architecture enables zero-downtime hot-swap when credentials arrive."),
        ("Third-Party SMS Gateway Downtime: Local SMS aggregator experiencing delivery latency.", "Medium", "Low", "Implement fallback email notification dispatch; queue SMS alerts via resilient background retry policies."),
        ("Inventory Concurrency Bottlenecks: High traffic spikes causing database lock contention.", "High", "Low", "PostgreSQL row-level locking (SELECT FOR UPDATE) combined with atomic quantity_reserved columns and index optimization."),
        ("Prescription Privacy Breaches: Unauthorized access to customer medical records.", "Critical", "Low", "Pre-signed short-lived URLs; strict cryptographic token validation; encrypted private cloud bucket isolation.")
    ]
    add_styled_table(doc, risk_headers, risk_data, col_widths=[2.2, 0.9, 0.9, 3.0])

    print("Writing Section 17: Conclusion & Strategic Recommendations...")

    # ==========================================
    # SECTION 17: CONCLUSION AND STRATEGIC RECOMMENDATIONS
    # ==========================================
    doc.add_heading("17. Conclusion & Strategic Recommendations", level=1)
    
    add_body(doc, "CareBasket (provisional working title) represents a highly sophisticated, commercially viable, and socially impactful software engineering initiative. By combining modern Clean Architecture in ASP.NET Core, the rock-solid transactional integrity of PostgreSQL 16+, and a lightweight, ultra-responsive Tailwind CSS storefront, the platform establishes a dependable foundation for digital healthcare commerce in Bangladesh.")

    add_body(doc, "Rather than attempting an overly broad initial launch laden with complex, unverified features, this proposal establishes a disciplined, phased development lifecycle. The initial release (Release 1.0) deliberately focuses on mastering the non-negotiable fundamentals: sub-second catalog search, absolute inventory consistency, an airtight clinical prescription review pipeline, localized address hierarchy, and transparent order tracking.")

    add_body(doc, "Once this robust transactional foundation is proven in production, the modular architectural design provides an effortless springboard for Phase 7 innovations, including direct bKash and Nagad production payment gateways, automated courier dispatching, and conversational AI-driven customer support operating within strict safety boundaries.")

    add_callout(doc, "Immediate Next Steps & Recommendation for Executive Action", [
        "1. Stakeholder Scope Approval: Formal review and stakeholder sign-off on the functional scope, deliverables, and architecture detailed in this proposal.",
        "2. Brand & Identity Finalization: Review of the provisional 'CareBasket' codename and finalization of registered corporate branding and domain assets.",
        "3. Phase 1 Inception Execution: Immediate commencement of Phase 1 activities, including user story mapping, PostgreSQL schema finalization, and wireframe sign-offs."
    ], kind="info")

    # Output file path
    output_filename = "Healthcare_Ecommerce_Project_Proposal.docx"
    doc.save(output_filename)
    print(f"Document successfully created and saved as: {output_filename}")
    return output_filename

if __name__ == "__main__":
    build_proposal_document()
