

FULL-TEXT SCREENING SPECIFICATION
(Kitchenham SLR + PRISMA compliant)

1. Objective
Full-Text Screening là bước đánh giá toàn bộ nội dung bài báo nhằm xác định liệu study có thực sự phù hợp với Research Protocol của Systematic Literature Review hay không.
Mục tiêu:
xác nhận các study phù hợp với Research Question
loại bỏ study không đáp ứng Inclusion/Exclusion Criteria
chuẩn bị tập study cho các bước tiếp theo:
Quality Assessment
Data Extraction
ghi nhận exclusion reasons để phục vụ PRISMA reporting
Reviewer phải đánh giá dựa trên:
Full paper content
Methodology
Population / Dataset
Intervention / Technique
Experiment / Study design
Results / Outcomes
Conclusion

2. Inputs
Một paper được chuyển vào Full-Text Screening khi:
đã Include ở bước Title/Abstract Screening
có full-text PDF hoặc full-text link
Input bao gồm:
Paper metadata
Full-text PDF
Research Protocol
Research Question
PICOC definition
Inclusion Criteria
Exclusion Criteria
Study Type definition
Time Range
Language constraints


3. Output
Kết quả của Full-Text Screening gồm:
Decision: Include | Exclude
Exclusion reason (if excluded)
Reviewer notes
Review timestamp

Paper chỉ được chuyển sang bước tiếp theo nếu:
Final Decision = Include

Sau đó paper sẽ được chuyển sang:
Quality Assessment
Data Extraction


4. Screening Criteria
Reviewer phải đánh giá paper dựa trên các yếu tố sau:
4.1 Research Question Alignment
Paper phải liên quan tới Research Question của SLR.

4.2 PICOC Matching
Paper được so sánh với PICOC:
Element
Description
Population
đối tượng nghiên cứu
Intervention
phương pháp / technique
Comparison
baseline / alternative
Outcome
kết quả / metric
Context
domain / environment


4.3 Study Type
Chỉ các empirical primary studies được chấp nhận.
Ví dụ accepted:
Experiment study
Case study
Controlled study
Field study
Empirical evaluation

Ví dụ rejected:
Opinion paper
Editorial
Position paper
Tutorial
Secondary study


4.4 Methodology validity
Paper phải có:
clear methodology
experiment or evaluation
dataset or participants
result analysis


4.5 Outcome relevance
Kết quả phải liên quan đến:
research objective
evaluation metrics
experiment results


5. Exclusion Reason Taxonomy
Nếu paper bị loại, reviewer phải chọn exclusion reason từ danh sách chuẩn.
Code
Reason
E1
Not relevant population
E2
Not relevant intervention
E3
Not relevant outcome
E4
Wrong study type
E5
Not empirical study
E6
No experiment / evaluation
E7
Not in time range
E8
Language not supported
E9
Duplicate study
E10
Full text not available

Reason codes giúp tạo PRISMA statistics.

6. Workflow
Step 1 — Paper eligibility
Chỉ paper có status:
Title/Abstract Screening = Include

được chuyển sang Full-Text Screening.

Step 2 — Full-text availability check
System kiểm tra:
Full-text PDF
OR
Full-text link

Nếu không có:
Status = Full-text missing

Reviewer có thể:
upload PDF manually
add full-text link


Step 3 — Protocol lock
Trong suốt quá trình screening:
Research Protocol = Locked

Không được thay đổi:
Research Question
PICOC
Inclusion Criteria
Exclusion Criteria
Study Type
Time Range

Điều này đảm bảo methodological consistency.

Step 4 — Reviewer assignment
Mỗi paper được review bởi:
minimum: 2 reviewers
maximum: 3 reviewers

Reviewers làm việc independently.

Step 5 — Independent review
Reviewer đọc:
Full paper
Methodology
Experiment
Results
Conclusion

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

Điều này giúp giảm selection bias.

Step 7 — Conflict detection
Sau khi reviewers submit:
Nếu decisions giống nhau:
Include + Include → Include
Exclude + Exclude → Exclude

Nếu decisions khác nhau:
Conflict

Paper sẽ được gửi cho Leader / Senior reviewer.

Step 8 — Conflict resolution
Leader đọc paper và đưa ra:
Final Decision

Decision này ghi đè tất cả decision trước đó.

Step 9 — Majority decision
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
Quality Assessment
Data Extraction

Nếu:
Exclude

paper không được quay lại pipeline.

7. AI-Assisted Screening (Optional)
AI có thể hỗ trợ reviewer nhưng không được phép đưa ra decision cuối cùng.
AI chỉ cung cấp:
Suggested decision
Relevance score
Confidence score


7.1 Relevance score
AI tính Relevance Score dựa trên nội dung full-text:
0.0 = completely irrelevant
1.0 = highly relevant


7.2 AI comparison with protocol
AI hiển thị kết quả so sánh:
Population → Match / Partial / Mismatch
Intervention → Match / Partial / Mismatch
Outcome → Match / Partial / Mismatch
Study Type → Match / Mismatch
Context → Match / Partial / Mismatch
Time Range → Match / Mismatch


7.3 AI content highlighting
AI highlight các phần quan trọng trong full-text:
Population
Methodology
Intervention
Experiment design
Outcome
Results

Mục tiêu:
giúp reviewer đọc nhanh hơn


7.4 Manual AI trigger
AI model chỉ chạy khi reviewer kích hoạt thủ công.
Nếu AI không hoạt động:
Reviewer vẫn có thể thực hiện screening thủ công


8. Duplicate Study Handling
Nếu nhiều paper báo cáo cùng một study:
conference version
journal extension

Reviewer phải giữ:
most complete version

Các paper còn lại bị exclude với reason:
E9 – Duplicate study


9. PRISMA Reporting Support
System phải ghi lại thống kê để tạo PRISMA flow diagram.
Ví dụ:
Records after title/abstract screening: 420
Full-text assessed: 210
Full-text excluded: 130

Breakdown:
E1 – Not relevant population: 25
E2 – Not relevant intervention: 30
E3 – Not relevant outcome: 18
E4 – Wrong study type: 22
E6 – No experiment: 15
E9 – Duplicate study: 10
E10 – No full text: 10


10. Audit Trail
System phải lưu lại:
Reviewer ID
Decision
Exclusion reason
Timestamp
Notes
AI suggestion (if used)

để đảm bảo:
traceability
replicability
transparency


11. Summary Workflow
Full pipeline:
Literature Search
      ↓
Duplicate Removal
      ↓
Title/Abstract Screening
      ↓
Full-Text Screening
      ↓
Quality Assessment
      ↓
Data Extraction
      ↓
Evidence Synthesis



