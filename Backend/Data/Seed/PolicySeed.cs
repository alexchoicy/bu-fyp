using Backend.Models;
using Backend.Services.AI;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public class PolicySeed
{
    public static async Task SeedAsync(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        if (await context.PolicySections.AnyAsync())
            return;

        await Policy1Creation(context, aiProviderFactory);
        await Policy2Creation(context, aiProviderFactory);
        await Policy3Creation(context, aiProviderFactory);
        await Policy4Creation(context, aiProviderFactory);
        await Policy5Creation(context, aiProviderFactory);
        await Policy6Creation(context, aiProviderFactory);
        await Policy7Creation(context, aiProviderFactory);
        await Policy8Creation(context, aiProviderFactory);
        await Policy9Creation(context, aiProviderFactory);
        await Policy10Creation(context, aiProviderFactory);
        await Policy11Creation(context, aiProviderFactory);
        await Policy12Creation(context, aiProviderFactory);
        await Policy13Creation(context, aiProviderFactory);
        await Policy14Creation(context, aiProviderFactory);
    }

    private static async Task Policy1Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "1",
            Heading = "1. Admission, Enrolment and Registration",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "1.1 Admission to undergraduate degree programmes is subject to satisfying both the stipulated General University Admission Requirements and Programme Admission Requirements as stipulated by the University.\n\n1.2 All students are required to register for courses according to their prescribed Study Schedule every semester. Students should complete the procedures according to the dates officially announced by the Academic Registry.\n\nRegistration of Courses\n\n1.3 Students will automatically be registered for courses required by their programmes of study. Registration of elective courses takes place in the respective pre-registration exercise in each semester. Students who fail to register for any of the course(s) indicated in the pre-registration exercise should complete the course registration during the course add/drop period.\n\nAdding/Dropping of Courses after Registration\n\n1.4 Adding/Dropping of courses is allowed during the course add/drop period.\n\n1.5 The University reserves the right to cancel courses with low enrolment anytime before the deadline for adding/dropping of courses.\n\nWithdrawing from Courses after the Period for Adding/Dropping of Courses\n\n1.6 Permission to withdraw from courses after the deadline for adding/dropping of courses will only be given under exceptional circumstances, such as illness, personal or academic problems, or other unforeseen circumstances deemed acceptable to the course instructors and the programmes concerned and the Academic Registrar.\n\n1.7 Applications should be endorsed by the course instructors and the programmes concerned and approved by the Academic Registrar at least four weeks before the commencement of the semester examinations."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy2Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "2",
            Heading = "2. Units and Study Load",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "Units\n\n2.1 The number of units assigned to a course is indicative of the total time spent by an average learner in all modes of learning, including lectures, self-study, and other teaching, learning and assessment component, to achieve the learning outcomes of the course, normally over a period of one semester.\n\n2.2 In general, a single unit corresponds to 45 notional learning hours.\n\nStudy Load\n\n2.3 Students should register for all courses prescribed by their programmes, including elective courses, as stipulated in the Study Schedules.\n\n2.4 Students who cannot fulfil the graduation requirements within the normal period of study and are granted extension of study, they will be classified as “part-time” when registering for less than 12 units in a semester during the extension period.\n\n2.5 Unless prescribed in an approved study plan, students may not register for more than 18 units in a semester. Students who wish to do so must obtain approval from the Department Head/Programme Director during the course add/drop period. Students who wish to register for more than 21 units must obtain approval from the Department Head/Programme Director and the Academic Registrar. Approval for a study load in excess of 21 units will only be given under exceptional circumstances.\n\n2.6 Students who are placed on academic probation are required to work out and agree on a study plan with their Department Head/Programme Director. Normally, the study plan will include a reduced study load according to the following guidelines:\n\nList (ordered):\\n1. Students with a failure of 0–6 units in the previous semester may be required to take not more than 12 units in the following semester.\\n2. Students with a failure of more than 6 units in the previous semester may be required to take not more than 9 units in the following semester.\n\n2.7 The President and Vice President(s) of the Students’ Union will be allowed to apply for leave of absence or reduce their study load to any number of units during their tenure in the Students’ Union, and to extend their study period up to one academic year if they so wish. In such cases, an appropriate study plan should be drawn up by the student concerned and approved by the Department Head/Programme Director.\n\nDouble Registration\n\n2.8 Unless otherwise approved by the University, undergraduate students are not permitted to enrol on any other part-time or full-time UGC-funded programmes at the University or at any other local institutions of higher learning. Students breaching this regulation are subject to discontinuation of their studies at the University."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }
    private static async Task Policy3Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "3",
            Heading = "3. Attendance",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "3.1 Students are expected to attend all scheduled classes for which they have registered. If absence is due to conditions beyond their control and they wish to establish that fact in order to justify make-up work (e.g. papers, assignments), a written explanation together with supporting documents must be presented to the course instructor for approval within five days after the absence."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy4Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "4",
            Heading = "4. Assessment",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "4.1 Students are assessed by different assessment tasks such as coursework assignment, essay, presentation, project and examinations etc., as specified in the programme document and course outlines.\n\nAcademic Honesty\n\n4.2 Any students who have committed an act of academic dishonesty during their course of studies at the University shall receive an “F” grade for the course. More serious or repeated cases may be referred to the Student Disciplinary Panel as appointed by the Student Affairs Committee for deliberation of whether more stringent disciplinary action would be taken.\n\nSemester Examinations\n\n4.3 Only students whose names are on the course enrolment record are permitted to sit for the semester examination of the course.\n\n4.4 Students should sit for all scheduled examinations. Students failing to do so without official permission will receive an \"F\" grade for the examination concerned.\n\n4.5 Normally, the end-of-semester or year examinations are scheduled within the University examination period by the Academic Registry as two- or three-hour closed-book written examinations. For alternative arrangements, the course instructor must submit the details with the approval of the Department Head/Programme Director to the Academic Registry for record.\n\nMake-up Examinations\n\n4.6 Students missing an examination because of extenuating circumstances such as illness, injury or other personal emergencies may apply in writing via an application form with supporting document(s) to the Academic Registrar for a make-up examination. Applications should be made within five working days after the missed examination.\n\n4.7 In case of illness or injury, the application must include a medical certificate recommending for sick leave on the date of the missed examination issued by a recognized medical practitioner.\n\n4.8 The following situations would not be considered:\n\nList (ordered):\n1. Elective surgery scheduled to be held on an examination day;\n2. Public examinations such as HKDSE Examination, TOEFL, SAT, GRE, GMAT, etc., held on an examination day; or\n3. Having forgotten or misread the examination schedule.\n\n4.9 If the application is approved, the make-up examination will normally be arranged by the Academic Registry within six weeks after the examination period.\n\n4.10 No other arrangement will be made if the student is unable to attend the make-up examination.\n\n4.11 The course instructor should set a new examination paper for the make-up examination and will decide if the grades for the make-up examination should be lowered.\n\n4.12 Students missing an examination who do not apply for a make-up examination or whose application is disapproved will receive a zero mark for that examination.\n\nSupplementary Examinations\n\n4.13 Students who fail a course only due to failure of the end-of-semester or year examination may be allowed to sit for a supplementary examination, as recommended by the Programme in consultation with the course instructor.\n\n4.14 Students, including final year students, are allowed to take supplementary examination for a maximum of one course per semester and Grade D is the maximum grade for the course after supplementary examination.\n\n4.15 For final year students, recommendation to take supplementary examination for any failed course in Semester 2 shall be approved by the Senate."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy5Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "5",
            Heading = "5. Examination Regulations",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "5.1 Students are not allowed to sit for an examination if they have not completed the proper course/section enrolment procedures.\n\n5.2 Students should read the examination timetable carefully and take note of the date, time and venue of the examinations. Having forgotten or misread the examination timetable is normally not an excuse to apply for make-up examinations.\n\n5.3 Students should arrive at the examination venue at least 10 minutes before the scheduled time of the examination. Once they enter the examination venue, they should sit according to the seat number assigned to them.\n\n5.4 Students will not be allowed to enter the examination venue after the first 30 minutes of the examination.\n\n5.5 Students are not allowed to leave the examination venue after they enter the examination venue (except with the permission of the invigilator), during the first 30 minutes and the last 15 minutes of the examination.\n\n5.6 Students should bring their Student ID Card (or HKID Card) and put it at the top right-hand corner of the desk throughout the examination. Students without any such identification may not be allowed to sit for the examination.\n\n5.7 Before entering the examination venue, students must make sure that unauthorized articles/items (e.g. books, manuscripts, notes, papers and all kinds of electronic/communication devices, including smart watches) are taken out from their pockets and placed inside their bags. All electronic/communication devices must be turned off.\n\n5.8 Once they enter the examination venue, students should place their bags under their seats immediately. They should also make sure that no unauthorized articles/items are put on the desk unless prior approval is given by the invigilator.\n\n5.9 Students should place their stationery on the desk and their pencil case/box under the seat.\n\n5.10 Students must not turn over the pages of the examination question paper and must not start working until they are instructed to do so.\n\n5.11 Students should remain absolutely silent once they enter the examination venue. They must not talk to or disturb other students. If they have questions, they should put up their hands and wait patiently for an invigilator.\n\n5.12 Students who wish to leave the examination venue temporarily during an examination session must:\n\nList (ordered):\n1. raise their hand and wait for an invigilator;\n2. leave only with the approval of the invigilator.\n\nBefore students leave the venue, the invigilator has the right to check whether any unauthorized articles/items have been placed in their pocket(s).\n\n5.13 Students who have completed their examination and wish to leave the venue early during an examination session must:\n\nList (ordered):\n1. first check the answer book(s) and papers that they bear their names, student numbers, course code and section number (even if no attempt has been made to answer any questions);\n2. raise their hand and wait for an invigilator;\n3. leave only with the approval of the invigilator.\n\n5.14 At the end of the examination, students must:\n\nList (ordered):\n1. promptly stop writing, put their pens down at once, remain seated and wait silently until the invigilator has collected all answer books;\n2. leave only when the chief invigilator tells them to do so;\n3. not remove anything from the examination venue except personal belongings and the question papers (if allowed).\n\nPenalty/Disqualification\n\n5.15 Students have the sole responsibility to ensure that the examination regulations are observed and complied with.\n\n5.16 Students who are found to have breached any of the examination regulations or committed the following offences shall be subject to penalty or disqualification and may be given an “F” grade for the course:\n\nList (ordered):\n1. Copying other students' work or any form of cheating inside or outside the examination venue;\n2. Having unauthorized articles/items on the examination desk, in the pockets or on their body after entering the examination venue and during the examination session;\n3. Removing articles/items other than personal belongings from the examination venue;\n4. Leaving the examination venue without permission;\n5. Disobeying instructions of an invigilator.\n\nIn addition, more serious or repeated cases shall be submitted to the Student Affairs Committee for further disciplinary action.\n\n5.17 Students who are absent from an examination without an acceptable reason and proper documentation evidence will receive zero mark for that examination.\n\nArrangement of Examinations on the Approach of Typhoon/Rainstorms\n\n5.18 Students should follow the arrangement of examinations due to bad weather conditions, which can be found on the Academic Registry web page.\n\n5.19 All examinations postponed due to bad weather conditions will be conducted at the same hours on the first working day after the last day of the examination. \n Notes:1. “Examination venue” is a generic term. When an examination is conducted online, examination venue refers to the online examination platform specified by the course instructor.\n\n2. Students should join the online examination platform using the name as printed on the student card."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading} {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy6Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "6",
            Heading = "6. Assessment Grading System",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk1 = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "6.1 Letter grades are used to indicate the results of assessment. The number of grade points gained by a student in a particular course corresponds to the letter grade.\n\nTable:\nLetter Grade | Academic Performance | Grade Point Per Unit\nA ) | Excellent | 4.00\nA- ) | 3.67\nB+ ) | Good | 3.33\nB ) | 3.00\nB- ) | 2.67\nC+ ) | Satisfactory | 2.33\nC ) | 2.00\nC- ) | 1.67\nD | Marginal Pass | 1.00\nE | Conditional Pass | 0.00\nF | Fail | 0.00\nDT | Distinction | Not included in grade point average (GPA)calculation\nI | Incomplete | Not included in GPAcalculation\nS | Satisfactory | Not included in GPAcalculation\nU | Unsatisfactory | Not included in GPAcalculation\nW | Withdrawn | Not included in GPAcalculation\nYR | Year Grade | Not included in GPAcalculation\nNR | Not Yet Reported | Not included in GPAcalculation\nPR | Project to be Resubmitted | Not included in GPAcalculation\n\nGrade A (i.e. A and A-) indicates that the students have an excellent performance on all Intended Learning Outcomes (ILOs) and a thorough mastery of the subject matter.\n\nGrade B (i.e. B+, B and B-) indicates that the students have a good performance on all ILOs and are competent in knowledge of the subject matter; or the students have an excellent performance on the majority of the ILOs and are competent in knowledge of the subject matter.\n\nGrade C (i.e. C+, C and C-) indicates that the students have a satisfactory performance on all ILOs and an acceptable level of knowledge of the course; or the students have a good performance on some ILOs which compensate for marginal performance on others, resulting in an overall satisfactory performance. In addition, the students should have an acceptable level of knowledge of the course.\n\nGrade D indicates that the students have a marginal acceptable performance on the majority of the ILOs and are permitted to proceed to more advanced work in the subject area.\n\nGrade E is a temporary grade applicable only to the first-semester component of a year course. Students who receive the conditional grade may continue to study the course in the following semester. If the students obtain a passing grade in the following semester, the first-semester grade E will be converted to grade D. In the case of failure (F grade), withdrawal from, or discontinuation of that course in the following semester, the first-semester grade E will be converted to grade F.\n\nGrade F indicates an unsatisfactory performance on the majority of the ILOs. Students with grade F in the first semester of a year course are not allowed to continue studies in that course in the following semester.\n\nGrade DT indicates that the students have an excellent performance on all ILOs and a thorough mastery of the subject matter. Grade DT is not included in the GPA calculation.\n\nGrade I is a temporary grade given only when the required work for the course has not been completed due to unavoidable reasons acceptable to the course instructor. If the work is not completed within six weeks after the official announcement of the course semester grades by the Academic Registry, the grade I will automatically be converted to grade F. Grade I is not included in the GPA calculation.\n\nGrade S is used to indicate satisfactory completion of a course. It is not included in the GPA calculation.\n\nGrade U is used to indicate unsatisfactory performance in a course. It is not included in the GPA calculation. The use of this grade has to be approved by the Senate.\n\nGrade W is used to indicate approved withdrawal from the course after the deadline for dropping of courses. Grade W is not included in the GPA calculation.\n\nGrade YR is a temporary grade applicable both to the first component of a year course and to courses that span over more than one academic year. The YR grade indicates that the students will be assessed after the course has been fully completed. Same grade or different grades will be assigned to the different components of the course. If the students drop or withdraw from the course in the last semester, the YR grade will be converted to grade W or any letter grade, depending on the extent to which the students’ performance up to the end of each semester preceding the last semester of the course can be assessed by the course instructor. Grade YR is not included in the GPA calculation.\n\nGrade NR is a temporary grade. The NR grade indicates that the grade for the course is not yet reported by the course instructor at the time the semester grade report is prepared. Grade NR is not included in the GPA calculation. The conversion of NR grade to a normal letter grade should be made within six weeks after the announcement of course grades."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk1.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading} - Part 1\n{chunk1.Content}";
                chunk1.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading} - Part 1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading} - Part 1: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk1);

        var chunk2 = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 1,
            Content = "Grade PR is a temporary grade applicable to final year students whose honours projects are found to be unsatisfactory on submission and on the recommendation of the respective Programme are granted an extension up to 1st November of the same year for resubmission. If the project is considered satisfactory on resubmission, the grade will be converted to a letter grade not higher than C. Grade PR is not included in the GPA calculation.\n\nGrade Point Average (GPA) and Retaking of Courses\n\n6.2 The GPA is an important indicator of the academic standing of students. It is obtained by adding all the grade points gained and then dividing the sum by the total number of units attempted.\n\n6.3 The semester GPA is calculated from all the grade points gained and the number of units attempted in a given semester. The cumulative GPA (cGPA) is calculated from the cumulative grade points gained and the cumulative number of units attempted.\n\n6.4 Students must obtain a passing grade on all courses prescribed by their programmes.\n\n6.5 Students may only repeat courses with grade F to retrieve the failure. Students, however, may be required by their programmes to repeat courses with less than satisfactory grades to fulfil specific course or programme requirements.\n\n6.6 For a course taken for more than once, only the highest grade* will be included in the calculation of cGPA with effect from the semester in which the highest grade is attained. The number of units gained for the repeated course is counted once only.\n\n* If the grades attained at different attempts are the same, the latest grade shall be included in the cGPA calculation.\n\n6.7 Students can only repeat the same course twice. There is no limit on the number of courses to be repeated."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk2.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading} - Part 2\n{chunk2.Content}";
                chunk2.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading} - Part 2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading} - Part 2: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk2);
        await context.SaveChangesAsync();
    }

    private static async Task Policy7Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "7",
            Heading = "7. Academic Results",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "7.1 Students’ academic results are officially recorded in the Academic Registry at the end of each semester. A grade report will be released approximately five to six weeks after the examination period. Students should report any inaccuracy or inconsistency to the Academic Registry within three weeks after the reports have been issued.\n\nAcademic Honours\n\n7.2 All students, including non-degree seeking students, taking a minimum of 12 units counting towards GPA and attaining the following results in a semester are eligible for the respective academic honours:\n\nTable:\na. | President’s Honour Roll: | With a semester GPA of 3.50 or above and with no grades below “C” for the same semester*.\nb. | Dean’s List: | With a semester GPA of 3.00–3.49 and with no grades below “C” for the same semester*.\n\n* Students with a “U” grade in the same semester are NOT eligible for receiving the academic honours.\n\nAcademic Problems\n\n7.3 Students taking 12 units or more counting towards GPA and attaining the following results in a semester will be given the respective course of action:\n\nTable:\na. | Academic Warning: | With a semester GPA between 1.67 and 1.99 for the same semester.\nb. | Academic Probation: | With a semester GPA below 1.67 for the same semester.\nc. | Academic Dismissal: | With a semester GPA of below 1.67 for two consecutive semesters; or on other academic grounds.\n\n7.4 Students taking less than 12 units counting towards GPA and attaining the following results in a semester will be given the respective course of action:\n\nTable:\na. | Academic Warning: | With a semester GPA below 2.00 for the same semester.\nb. | Academic Probation: | With a semester GPA below 1.67 for two consecutive semesters.\nc. | Academic Dismissal: | With a semester GPA of below 1.67 for three consecutive semesters; or on other academic grounds.\n\nRepeat Study\n\n7.5 Students with poor academic results may, at the discretion of individual programmes, be required to repeat a year of study with the approval of the Senate.\n\n7.6 Students are required to fulfil any study conditions as prescribed by their programmes during their repeating year. If they fail to meet the conditions, they may be recommended to the Senate for dismissal by the programmes.\n\n7.7 Students, during their entire study at the University, are allowed to repeat only once in either Year 1, 2 or 3. Students who fail to fulfil the graduation requirements after the final year of study may also be granted concession of one additional year of study with the approval of the Senate."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy8Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "8",
            Heading = "8. Course Exemption",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "8.1 Students admitted to Year 1 may apply for exemption from taking certain courses in the curriculum if they have taken equivalent courses in other institutions before, but they are required to take other courses to make up the units being exempted. Students should submit their applications for course exemption within two weeks of their first semester at the University."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy9Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "9",
            Heading = "9. Transfer of Units",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "9.1 Students admitted to Year 1 with advanced standing may be granted up to 15 units of transferred units at the point of admission.\n\n9.2 For minor programmes, the number of units transferred from other institutions is limited to 6 units.\n\n9.3 For students taking the double major programmes (DMP)1, the number of units transferred from another institution for the second major (DMP-SM) is limited to 40% of the unit requirement for the DMP-SM.\n\n9.4 Normally, the students’ transferred units (including units granted at the point of admission and units that students obtained through exchange programmes) should not be more than one-half of the total units required for graduation. Senior Year entrants admitted directly to Year 3 may receive transferred units of not more than one-half of the total units required for graduation at the point of admission. For those who further participate in exchange programmes, they may receive transferred units of up to two-thirds of the units required for graduation.\n\n9.5 The University reserves the right not to grant units for courses which are not deemed to be equivalent to the University courses and for courses with grades below the equivalence of grade C in the University grading system.\n\n9.6 Students may be required to sit for proficiency test(s) or qualifying examination(s) prior to the granting of transfer units.\n\n9.7 Units transferred from other institutions are recorded without inclusion in GPA calculations.\n\n_____\n\n1 offered by the Faculty of Arts and Social Sciences"
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy10Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "10",
            Heading = "10. Double-counting of Courses",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "10.1 Double-counting of courses is only permissible under the following situations:\n\nList (ordered):\n1. Double-counting of courses up to a maximum of six units is permissible between the major (including first major (FM), which is applicable to students graduating with a specific Transdisciplinary Second Major (TSM) or Second Major (SM) from the list of approved TSMs/SMs as shown in the Student Handbook) courses and the interdisciplinary concentration courses.\n2. Double-counting of courses of three units is permissible between the major (including FM), minor or concentration courses and the GE Capstone.\n3. Double-counting of courses (at Level 2 or below) up to a maximum of nine units is permissible between the FM courses and the TSM or SM courses for students graduating with a TSM or SM.\n4. Double-counting of courses up to a maximum of six units is permissible between the GE Level 2 courses and the TSM courses, including three units of the TSM common core course and three units of GE Level 2 course specified in the Study Schedule of the TSM for students graduating with a TSM.\n5. Any other double-counting of courses between major courses of individual programmes and GE courses as approved.\n\n10.2 In fulfilling the overall units required for graduation, the units of the double-counted courses shall only be counted once and students will have to take other courses to make up the units."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy11Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "11",
            Heading = "11. Change of Study Programme",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "11.1 Non-final year students may apply for change of study programme in April each year. No application will be entertained in other time of the academic year.\n\n11.2 Students are normally expected to have met the admission requirements of the new study programme and to have attained a cumulative GPA of not less than 2.0 in their studies in order to proceed with the application.\n\n11.3 All applications shall be approved by the Department Heads/Programme Directors and Deans of both the transfer-out and transfer-in programmes. The applications will first be sent to the transfer-out programmes for approval. Only applications that are approved by the transfer-out programmes will be passed to the transfer-in programmes for consideration.\n\n11.4 If the applications are approved, the transfer-in programmes will decide to admit the students to the year of study which they consider appropriate.\n\n11.5 The transfer will take effect in the following academic year. The transfer-in programmes shall send the new study plans of the students to the Academic Registry before the start of the students’ study.\n\n11.6 Students are only allowed to change the study programmes once throughout their undergraduate studies."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy12Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "12",
            Heading = "12. Leave of Absence, Suspension, Withdrawal and Dismissal",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "Leave of Absence\n\n12.1 Students may be permitted to take leave of absence from their studies in cases of health problems, study-related placement, or other circumstances as deemed acceptable by their programmes.\n\n12.2 In all cases, a written application together with supporting documents must be submitted to the Academic Registry by the deadline stipulated by the Academic Registry. The Academic Registrar will consult with the Department Head/Programme Director before making a decision. A leave period up to two semesters may be granted for each application approved. For further extension of the leave period, a new application has to be made before the expiration of the leave period.\n\n12.3 The Academic Registry will inform the student of the result of applications. If the applications are approved, the students will be notified of the approved period of leave of absence.\n\n12.4 Before expiration of the leave period, students must apply to the Academic Registry for resumption of studies. For leave of absence approved on medical grounds, medical documents certifying that the students are fit for study should be submitted at the time of application.\n\nSuspension\n\n12.5 Students may be required by the University to have their studies suspended for a period up to two semesters as a disciplinary sanction.\n\n12.6 Certain conditions may be applicable to the students in which they must satisfy before they are allowed to resume studies and/or after they have resumed studies. If the students cannot meet such conditions, they may be required to further suspend studies or withdraw from the University.\n\n12.7 For suspension period lasting for one semester or more, a remark regarding the student’s suspension of studies will be shown on the academic transcript.\n\n12.8 Before expiration of the suspension period, students must apply to the Academic Registry for resumption of studies.\n\nWithdrawal\n\n12.9 Students intending to withdraw from their studies at the University prior to graduation must apply for official withdrawal. Students who fail to follow the proper procedures will be considered as having unofficially withdrawn. No official documents will be issued to such students, and they will not be re-admitted in the future to any programme.\n\n12.10 For official withdrawal, students must complete the clearance procedures at the department/programme office, Library, Office of Student Affairs, Finance Office and Academic Registry. Students must settle any outstanding tuition fee before an official withdrawal status can be granted.\n\nDismissal\n\n12.11 The University may at any time, by action of the Senate, require any student to terminate their study at the University either on academic or disciplinary grounds, or on other grounds deemed appropriate.\n\n12.12 The Senate may also dismiss a student whose conduct or general influence is considered harmful to the institution. Such a student will normally not be considered for re-admission.\n\nRe-instatement\n\n12.13 A student who has withdrawn from the University for reasons other than academic ones may apply in writing to the Academic Registrar for re-instatement as a student."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }

    private static async Task Policy13Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "13",
            Heading = "13. Graduation, Graduation Honours and Honours Classifications",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk0 = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "Graduation Requirements\n\n13.1 Students are approved for graduation by the Senate after fulfilling all the graduation requirements stipulated by the University. These requirements include: University general requirements, programme requirements, and unit and GPA requirements. Students who have course, unit or GPA deficiencies may be granted summer study or supplementary examination or a concession of one additional year to complete their studies by the Senate.\n\n13.2 Individual programmes are responsible for checking the fulfilment of the graduation requirements of their students and the Academic Registry provides a counter-checking mechanism.\n\nUniversity General Requirements\n\nFor students admitted to Year 1 in the academic year 2024/25 or before, senior year entry students admitted to Year 2 in the academic year 2025/26 or before, and senior year entry students admitted to Year 3 in the academic year 2026/27 or before\n\n13.3 Undergraduate students are required to complete the following University general requirements:\n\nList (ordered):\n1. The minimum units required for graduation as specified in the Student Handbook.\n2. A minimum of 36 units of courses (including honours project) at Levels 3 and 4.\n3. University Core (13 units): 6 units of University English I and II, 3 units of University Chinese, 2 units of Healthy Lifestyle, and 2 units of Art of Persuasion.\n4. General Education courses (18 units): 9 units of Foundational Courses, 6 units of Interdisciplinary Thematic Courses and 3 units of GE Capstone.\n5. The non-credit bearing programme: University Life.\n6. Besides, students are required to reach foundation Putonghua proficiency before they graduate. Students must fulfil one of the following requirements for graduation:\n\nTable:\ni. | Pass a 3-unit Putonghua course which is valid for fulfilling the Putonghua requirement; or\nii. | Take a 25-hour non-credit bearing preparation course AND pass the Putonghua Proficiency Test conducted by the Language Centre. Students who fail the test must take the 3-unit Putonghua course.\n\n13.4 Students may apply for exemption from the above Putonghua requirement if they meet one of the following criteria:\n\nList (ordered):\n1. Non-Chinese speaking students such as those from foreign countries and international schools in Hong Kong who have been granted exemption from taking UCLC1005 University Chinese.\n2. Students who have attended the Chinese Language examination in the Tertiary Entrance Examination or Multi-admission Programme conducted by the Ministry of Education in the Mainland or Taiwan respectively.\n3. Students who have attained Grade C or above in the Hong Kong Certificate of Education Examination Putonghua subject.\n4. Students who have passed the Test of Proficiency in Putonghua conducted by the Hong Kong Examinations and Assessment Authority.\n5. Students who attain third class, Grade A or above in the Putonghua Shuiping Ceshi (a National Putonghua Proficiency Test).\n6. Students who have obtained a qualification that is deemed equivalent to one of the above by the Language Centre.\n7. Students who have passed the Diagnostic cum Exemption Test conducted by the Language Centre.\n\nFor students admitted to Year 1 in the academic year 2025/26 or after, senior year entry students admitted to Year 2 in the academic year 2026/27 or after, and senior year entry students admitted to Year 3 in the academic year 2027/28 or after\n\n13.5 Undergraduate students are required to complete the following University general requirements:\n\nList (ordered):\n1. The minimum units required for graduation as specified in the Student Handbook.\n2. A minimum of 36 units of courses (including honours project) at Levels 3 and 4. For students graduating with a TSM or SM, 21 of the units of courses shall be from the FM and the remaining 15 units of courses shall be from the TSM or SM.\n3. University Language Requirement (9 units): 6 units of University English I and II and 3 units of University Chinese and any other English Proficiency Enhancement Course(s) prescribed by the Language Centre as appropriate.\n4. General Education courses (22 units): 13 units of Foundational Courses, 6 units of Thematic Courses and 3 units of GE Capstone.\n5. The non-credit bearing programme: University Life.\n\nProgramme Requirements\n\n13.6 Students are required to fulfil their respective programme requirements.\n\n13.7 The following applies to students who wish to graduate with a single disciplinary major, a minor and/or interdisciplinary/transdisciplinary concentration."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk0.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading} - Part 1\n{chunk0.Content}";
                chunk0.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading} - Part 1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading} - Part 1: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk0);

        var chunk1 = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 1,
            Content = "List (ordered):\n1. Students satisfying the graduation requirements of the single disciplinary major programme but failing to satisfy the minor and/or interdisciplinary/transdisciplinary concentration requirements will graduate with single disciplinary major only. No extension of study period will be allowed for such students for the purpose of fulfilling the minor and/or interdisciplinary/transdisciplinary concentration requirements.\n2. Students satisfying the minor and/or interdisciplinary/transdisciplinary concentration requirements but failing to satisfy the graduation requirements of the single disciplinary major programme cannot graduate with the minor and/or interdisciplinary concentration only.\n\n13.8 The following applies to students who wish to graduate with FM and TSM or FM and SM, and a minor and/or interdisciplinary/transdisciplinary concentration:\n\nList (ordered):\n1. Students satisfying the requirements of the FM but failing to satisfy the TSM or SM requirement will not be allowed to graduate. However, the students may be allowed to switch back to the single disciplinary major. Upon the completion of the requirements of the single disciplinary major, students will graduate with the single disciplinary major only.\n2. Students satisfying the minor and/or interdisciplinary/transdisciplinary concentration requirements but failing to satisfy the graduation requirements of the FM and TSM or FM and SM cannot graduate with the minor and/or interdisciplinary/transdisciplinary concentration only.\n\n13.9 The following applies to students who wish to graduate with a second major under the double major programme (DMP-SM), a minor and/or interdisciplinary/transdisciplinary concentration:\n\nList (ordered):\n1. Students satisfying the graduation requirements of the home major under the DMP but failing to satisfy the DMP-SM, minor and/or interdisciplinary/transdisciplinary concentration requirements will graduate with the home major under the DMP only. No extension of study period shall be allowed for the purpose of fulfilling the DMP-SM, minor and/or interdisciplinary/transdisciplinary concentration requirements.\n2. Students satisfying the DMP-SM, minor and/or interdisciplinary/transdisciplinary concentration requirements but failing to satisfy the graduation requirements of the home major under DMP cannot graduate with the DMP-SM, minor and/or interdisciplinary/transdisciplinary concentration only.\n\nAttendance, Unit and GPA Requirements\n\n13.10 In addition to the University general requirements and programme requirements, students must meet the following requirements for the award of degrees:\n\nList (ordered):\n1. to have enrolled in the University for at least four years, or as specified by the programme requirements (for students admitted to advanced standing, the period of attendance may be reduced accordingly);\n2. to have successfully obtained the total number of units required by the programme, subject to fulfilling all University general requirements and programme requirements; and\n3. to have attained a minimum cumulative GPA of 2.00 for all courses attempted and have passed all courses stipulated by the programme.\n\nSupplementary Examination and Summer Study\n\n13.11 Students who do not satisfy the graduation requirements may be allowed to take summer study and supplementary examination in order to make up for their unit- or GPA-deficiency for graduation. The F grade will be replaced (if applicable) after the supplementary examination and the maximum grade given is D.\n\n13.12 Students must attain a cumulative GPA of 2.00 or above on all courses attempted including summer study and supplementary examination for graduation.\n\n13.13 The availability of summer study and supplementary examination is subject to the arrangements of the programmes concerned.\n\nUnsatisfactory Submission of Honours Project\n\n13.14 Students whose Honours Project submitted is unsatisfactory may, at the discretion of the programmes concerned, be given an extension until 1st November of the same year for resubmission. Students will only be considered for graduation in the same year if the final grade reaches the Academic Registry on or before 1st September. The final grade should not be higher than C.\n\nConcessional Year of Study\n\n13.15 Students who cannot complete the graduation requirements within the normal period of study of the programme because of academic problems, may be allowed a concession of one additional year to complete their studies, subject to the approval of the Senate. The additional year of study should immediately follow the students’ final year of study at the University.\n\nHonours Classifications\n\n13.16 There are two different systems of classification for degree programmes, one for Honours Degree programmes and the other for General (Non-Honours) Degree programmes."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk1.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading} - Part 2\n{chunk1.Content}";
                chunk1.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading} - Part 2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading} - Part 2: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk1);

        var chunk2 = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 2,
            Content = "13.17 Students enrolled on Honours Degree programmes will be awarded, upon completion of all requirements, the appropriate Bachelor’s degree with one of the following classifications:\n\nFirst Class HonoursSecond Class (Division I) HonoursSecond Class (Division II) HonoursThird Class HonoursPass\n\nIn the case of a Pass, the General Degree with a Pass will be awarded.\n\n13.18 The various classifications are based on the cumulative GPA. The general guidelines are as follows:\n\nTable:\ncGPA | Honours Classification\n3.40–4.00 | First Class\n3.00–3.39 | Second Class (Division I)\n2.50–2.99 | Second Class (Division II)\n2.20–2.49 | Third Class\n2.00–2.19 | Pass\n\n13.19 For programmes which award the General Degree as a distinct pathway option open to students, the cumulative GPA is again employed to determine different designations:\n\nTable:\ncGPA | Designation of Degree\n3.40–4.00 | Degree with Distinction\n2.67–3.39 | Degree with Merit\n2.00–2.66 | Pass Degree\n\n13.20 In all cases of classification/designation of degrees, the cumulative GPAs cited above are indicative. The Senate reserves the right, upon the recommendation of the Programme Management Committee concerned, to make exceptions in the application of these indicative GPAs.\n\n13.21 Individual faculties/schools may develop additional or alternative indicators for the award classifications in the programmes.\n\nScholastic Awards\n\n13.22 Student(s) having been awarded the First Class Honours and is the top two percent of graduate(s) (rounding to the nearest integer, but at least one award would be given for each programme/major (including FM)) with the highest cumulative GPA (cGPA) in the graduating class of their programme/major (including FM) will be granted the Scholastic Award of that programme/major (including FM) for that year.\n\n13.23 If the percentage of graduates eligible for the award exceeds the limit of two percent because there are graduates with the identical cGPA, all the eligible graduates will be granted the Scholastic Award of that programme/major (including FM).\n\n13.24 A programme/major (including FM) with no graduate awarded the First Class Honours may provide strong justifications on academic grounds to nominate, for consideration of the Senate, the graduate who has attained the highest cGPA, which is not less than 3.20, in the programme/major (including FM) to receive the Scholastic Award.\n\n13.25 The Senate reserves the right not to grant Scholastic Award to students who have satisfied the above award criteria for reasons including a breach of the University's standards of conduct.\n\nDiploma\n\n13.26 Graduates having completed all graduation requirements upon approval of the Senate will be given the relevant diploma as the official document of graduation. Diploma for graduates with outstanding fees to the University will be withheld until the fees are settled."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk2.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading} - Part 3\n{chunk2.Content}";
                chunk2.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading} - Part 3");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading} - Part 3: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk2);
        await context.SaveChangesAsync();
    }

    private static async Task Policy14Creation(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var policySection = new PolicySection
        {
            SectionId = "14",
            Heading = "14. Student Enquiries and Appeals Regarding Academic Matters",
            DocTitle = "General Regulations for Undergraduate Degree Programmes",
        };

        await context.PolicySections.AddAsync(policySection);
        await context.SaveChangesAsync();

        var chunk = new PolicySectionChunk
        {
            PolicySectionKey = policySection.PolicySectionKey,
            ChunkIndex = 0,
            Content = "14.1 Students may address queries on academic matters to the Academic Registrar, the respective Deans of Faculty/School, the respective Department Heads/Programme Directors, or any member of the teaching staff as relevant. The usual channel is for students to consult the course instructor when the query is about work in a particular course, the Department Head/Programme Director when the matter is related to the programme as a whole, and the Academic Registrar when the query concerns academic policies and procedures. If the query has the potential to become a matter for appeal, students should submit their official enquiries in writing to the Academic Registry.\n\nStudent Appeals against Course-Based Assessment\n\n14.2 Students who wish to appeal against the result of course-based assessment should follow the following procedures:\n\nList (ordered):\n1. Students who wish to appeal against course-based assessment including examination grades should first appeal in writing by giving a valid reason, which shall be limited to technical errors and procedural faults only, to the course instructor and Department Head/Programme Director concerned within eight working days after students are notified of the course semester grades. Students are required to pay a fee for lodging an appeal. If after reviewing the appeal, a technical error or procedural fault is confirmed by the Faculty/School Dean, the fee will be refunded to the student. Appeals without a valid reason will not be processed.\n2. The course instructor, or a review panel as decided by the Department Head/Programme Director, shall review the case and report to the Department Head/Programme Director, giving explanations. The students should be informed of the decision within three weeks from the day the appeal was lodged. Any changes in grades should be approved by the Faculty/School Dean before it is reported to the Academic Registry. The decision of the Faculty/School is final.\n\nStudent Appeals against Academic Decisions (Repeat a Year of Study and Dismissal)\n\n14.3 Upon the recommendation of the Department Head/Programme Director, the Senate may require students with poor academic results to repeat a year of study or be dismissed from the University. Students who wish to appeal against such academic decisions should lodge a formal appeal by writing to the Academic Registrar via their department/programme before the deadline set for the completion of clearance procedures, giving full reasons and providing documentations in support of the appeal. A fee will be charged for the appeal.\n\n14.4 The Department Head/Programme Director should submit a recommendation to support, or otherwise, the students' appeals to the Academic Registrar, who will then determine whether the appeals should be dismissed, or submitted for review and final decision by an Appeal Panel.\n\nAny recommendation of the Appeal Panel to revoke Senate decision shall be subject to ratification by Senate.\n\n14.5 The Composition of the Appeal Panel shall be as follows:\n\nList (ordered):\n1. Chairperson of the Appeal Panel—Chairperson of the Undergraduate Regulations Committee; or in the absence of the Chairperson, a senior academic nominated by the Academic Registrar;\n2. One senior academic nominated by the Chairperson;\n3. The Academic Registrar."
        };

        if (aiProviderFactory != null && !string.IsNullOrEmpty(chunk.Content))
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            try
            {
                var embeddingText = $"{policySection.Heading}\n{chunk.Content}";
                chunk.Embedding = await aiProvider.CreateEmbeddingAsync(embeddingText);
                Console.WriteLine($"Generated embedding for policy section: {policySection.Heading}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate embedding for policy section {policySection.Heading}: {ex.Message}");
            }
        }

        await context.PolicySectionChunks.AddAsync(chunk);
        await context.SaveChangesAsync();
    }
}
