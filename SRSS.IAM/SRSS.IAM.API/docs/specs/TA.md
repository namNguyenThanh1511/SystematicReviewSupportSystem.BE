
TITLE–ABSTRACT SCREENING SPECIFICATION
(Kitchenham SLR + PRISMA compliant)

1. Objective
Title–Abstract Screening là bước lọc sơ bộ các nghiên cứu dựa trên thông tin ngắn gọn của bài báo nhằm:
loại bỏ các paper không liên quan đến research topic
giảm số lượng paper trước khi Full-Text Screening
đảm bảo các paper tuân theo Research Protocol
Ở giai đoạn này reviewer không cần đọc full paper, chỉ cần đọc:
Title
Abstract
Keywords
Basic metadata


2. Inputs
Paper được đưa vào Title–Abstract Screening sau khi hoàn thành bước:
Identification Process


Input của bước này gồm:
Paper metadata
Title
Abstract
Keywords
Publication year
Language
DOI (optional)
Authors (optional)

Ngoài ra hệ thống phải có:
Research Protocol
Research Questions
PICOC definition
Inclusion Criteria
Exclusion Criteria
Study Selection Criteria
Time Range
Language constraints


3. Outputs
Kết quả của Title–Abstract Screening:
Decision: Include | Exclude
Exclusion Reason (if excluded)
Reviewer notes (optional)
Review timestamp

Paper chỉ được chuyển sang bước tiếp theo nếu:
Decision = Include

Sau đó paper được chuyển sang:
Full-Text Screening


4. Screening Criteria
Reviewer phải đánh giá paper dựa trên các yếu tố sau.

4.1 Topic relevance
Title và Abstract phải liên quan đến Research Question của SLR.

4.2 PICOC alignment
Paper được so sánh với PICOC framework.
Element
Meaning
Population
đối tượng nghiên cứu
Intervention
phương pháp / technique
Comparison
baseline / alternative
Outcome
kết quả / evaluation
Context
môi trường / domain

Ở giai đoạn Title–Abstract:
PICOC matching có thể partial

vì thông tin còn hạn chế.

4.3 Inclusion criteria
Paper phải thỏa mãn các Inclusion Criteria trong protocol.
Ví dụ:
I1 – Study addresses the research problem
I2 – Paper evaluates a method or technique
I3 – Study reports empirical results


4.4 Exclusion criteria
Paper bị loại nếu vi phạm Exclusion Criteria.
Ví dụ:
E1 – Not related to research topic
E2 – Not a research paper
E3 – Not empirical study
E4 – Outside time range
E5 – Not in supported language


4.5 Time range
Paper phải nằm trong khoảng thời gian được định nghĩa trong protocol.
Ví dụ:
2015 – 2024


4.6 Language constraint
Paper phải thuộc ngôn ngữ được hỗ trợ.
Ví dụ:
English


5. Exclusion Reason Taxonomy
Nếu paper bị loại, reviewer phải chọn exclusion reason chuẩn hóa.
Code
Reason
E1
Not relevant to research topic
E2
Not relevant population
E3
Not relevant intervention
E4
Not empirical study
E5
Not research paper
E6
Outside time range
E7
Unsupported language
E8
Duplicate study

Các reason codes được dùng để tạo PRISMA statistics.

6. Workflow

Step 1 — Protocol finalization
Trước khi bắt đầu screening:
Research Protocol must be finalized

Protocol bao gồm:
Research Questions
PICOC
Inclusion Criteria
Exclusion Criteria
Time Range
Language constraints

Sau khi screening bắt đầu:
Protocol must be locked

Không được thay đổi protocol.

Step 2 — Project activation
Project phải:
Activated
Screening process started
Reviewers invited


Step 3 — Data preparation
Trước screening hệ thống phải đảm bảo:
Papers imported
Duplicates removed
Metadata validated

Minimum metadata:
Title
Abstract
Year
Language

Optional metadata:
DOI
Authors
Keywords


Step 4 — Reviewer assignment
Mỗi paper được review bởi:
minimum: 2 reviewers
maximum: 3 reviewers

Reviewers làm việc independently.

Step 5 — Independent screening
Reviewer đọc:
Title
Abstract
Keywords

Sau đó chọn decision:
Include
Exclude

Nếu chọn Exclude, reviewer phải chọn:
Exclusion reason


Step 6 — Decision submission rules
Rules:
Reviewer chỉ submit 1 decision cho mỗi paper
Decision không thể chỉnh sửa sau khi submit
Reviewer không được xem decision của reviewer khác trước khi submit

Điều này đảm bảo independent screening.

Step 7 — Conflict detection
Sau khi reviewers submit decision:
Nếu decisions giống nhau:
Include + Include → Include
Exclude + Exclude → Exclude

Nếu decisions khác nhau:
Conflict

Paper được chuyển tới Leader / Senior reviewer.

Step 8 — Conflict resolution
Leader đọc lại paper và đưa ra:
Final Decision

Decision này sẽ ghi đè các decision trước đó.

Step 9 — Majority decision (optional)
Nếu có 3 reviewers:
Decision chiếm đa số được chọn

Ví dụ:
Include + Include + Exclude → Include


Step 10 — Final decision
Paper được gán:
Final Decision = Include | Exclude

Nếu:
Include

paper được chuyển sang:
Full-Text Screening

Nếu:
Exclude

paper không quay lại screening stage.

7. AI-Assisted Screening (Optional)
AI có thể hỗ trợ reviewer nhưng không được phép đưa ra decision cuối cùng.
AI chỉ cung cấp:
Suggested decision
Relevance score
Confidence score


7.1 Relevance score
AI tính Relevance Score dựa trên:
Title
Abstract
Keywords

Score range:
0.0 = completely irrelevant
1.0 = highly relevant


7.2 AI comparison with protocol
AI hiển thị kết quả so sánh:
Language → Match / Mismatch
Time Range → Match / Mismatch
Population → Match / Partial / Unknown
Intervention → Match / Partial / Unknown
Outcome → Match / Partial / Unknown
Context → Match / Partial / Unknown


7.3 AI keyword highlighting
AI highlight:
keywords liên quan tới research topic
terms liên quan tới PICOC
phrases liên quan tới inclusion/exclusion criteria

Điều này giúp reviewer đọc nhanh hơn.

7.4 AI prioritization
System có thể cho phép:
sort papers by relevance score

nhằm ưu tiên review các paper có khả năng liên quan cao.

7.5 Manual AI activation
AI model chỉ chạy khi reviewer kích hoạt thủ công.
Nếu AI không hoạt động:
screening vẫn phải hoạt động bình thường


8. PRISMA Reporting Support
System phải lưu thống kê để tạo PRISMA flow diagram.
Ví dụ:
Records identified from databases: 1200
Records after duplicates removed: 980
Records screened (title/abstract): 980
Records excluded: 750

Breakdown reasons:
Not relevant topic: 400
Not empirical study: 200
Outside time range: 80
Unsupported language: 70


9. Audit Trail
System phải lưu log cho mỗi decision:
Paper ID
Reviewer ID
Decision
Exclusion reason
Timestamp
Reviewer notes
AI suggestion (if used)

Mục tiêu:
transparency
traceability
replicability


10. Screening Pipeline
Toàn bộ pipeline SLR:
Literature Search
      ↓
Duplicate Removal
      ↓
Title–Abstract Screening
      ↓
Full-Text Screening
      ↓
Quality Assessment
      ↓
Data Extraction
      ↓
Evidence Synthesis



