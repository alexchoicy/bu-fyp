using Backend.Models;
using Backend.Services.AI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public class CourseSeed
{
    public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager, IAIProviderFactory? aiProviderFactory = null)
    {
        if (await context.Courses.AnyAsync())
            return;

        await seedCourses(context, userManager);
    }

    public static async Task<CourseVersion> comp1005Gen(AppDbContext context, Code compCode, Term semester1)
    {
        var comp1005 = new Course
        {
            Name = "Essence of Computing",
            CourseNumber = "1005",
            CodeId = compCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(comp1005);

        var comp1005Version = new CourseVersion
        {
            Course = comp1005,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "This course provides students with an overview of Information & Communication\nTechnologies, together with basic knowledge of computer-oriented problem solving\nmethodologies, algorithm development, general programming concepts and design\ntechniques.  Programming methods are introduced in a problem-solving approach using\na high-level programming language.\nUpon completion, students are expected to have an exposure to various areas of\nInformation & Communication Technologies. Students will gain experience in developing\nand implementing algorithms in a form of computer programs for solving real-world\napplications.",
            CourseContent = "I. Programming Fundamentals\n   - Primitive data types, operators and expressions\n   - Control structures\n   - Repetition structure such as for loops\n - Composite structure such as arrays\n   - Programming style, program testing and documentation\nII. Digital Media Computing and Data Communications\n   - Image processing applications and basic concepts behind\n   - Computer graphics, virtual reality and their applications\n   - Data communications and its applications\nIII. Web Technology and Data Analysis\n   - New opportunities from new Web technologies\n   - Basic concepts in Web technology and architecture\n   - Data analysis and its applications",
            CILOs = new List<CILOs>
        {
            new CILOs
            {
                code = "CILO1",
                Description = "Describe some fundamental concepts and real life applications in various areas of Information & Communication Technologies"
            },
            new CILOs
            {
                code = "CILO2",
                Description = "Describe the general elements in programming using a high level programming language"
            },
            new CILOs
            {
                code = "CILO3",
                Description = "Describe and explain the importance of programming development"
            },
            new CILOs
            {
                code = "CILO4",
                Description = "Analyze computational problems for solving practical problems using a high level programming language"
            },
            new CILOs
            {
                code = "CILO5",
                Description = "Formulate problems as steps so as to be solved systematically"
            },
            new CILOs
            {
                code = "CILO6",
                Description = "Build up and apply analytical thinking in the context of ICT and programming"
            }
        },
            TLAs = new List<TLAs>
        {
            new TLAs
            {
                code = new[] { "Lecture" },
                Description = "Students will attend lectures to learn different concepts in various areas of Information & Communication Technologies, as well as the general principles of programming and problem solving."
            },
            new TLAs
            {
                code = new[] { "Laboratory" },
                Description = "Students will attend laboratory sessions to gain practical exposure to various areas of Information & Communication Technologies and gain practical experience in programming."
            },
            new TLAs
            {
                code = new[] { "Assignment" },
                Description = "Students will work on programming exercises and assignments to enhance what they have learnt."
            }
        },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp1005Version);

        var comp1005Assessments = new List<CourseAssessment>
    {
        new CourseAssessment
        {
            CourseVersion = comp1005Version,
            Name = "Lab and Lecture Exercises",
            Weighting = 8,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = comp1005Version,
            Name = "Practical Tests",
            Weighting = 12,
            Category = AssessmentCategory.Examination
        },
        new CourseAssessment
        {
            CourseVersion = comp1005Version,
            Name = "Project",
            Weighting = 30,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = comp1005Version,
            Name = "Examination",
            Weighting = 50,
            Category = AssessmentCategory.Examination
        }
    };

        await context.CourseAssessments.AddRangeAsync(comp1005Assessments);

        await context.SaveChangesAsync();

        return comp1005Version;
    }

    public static async Task<CourseVersion> math1025Gen(AppDbContext context, Code mathCode, Term semester1)
    {
        var math1025 = new Course
        {
            Name = "Understanding Mathematics and Statistics",
            CourseNumber = "1025",
            CodeId = mathCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(math1025);

        var math1025Version = new CourseVersion
        {
            Course = math1025,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "This course applies concepts of pre-university level mathematics and statistics to solve real\nlife problems. Instead of drilling questions, students will learn problem-solving\nmethodologies and hands-on ICT (information and communication technology)  skills to\nsolve problems with computer software. The perspectives in this course are important for\nSTEM (science, technology, engineering and mathematics)  fields and quantitative aspects\nof business and economics, as well as for everyday statistical literacy.",
            CourseContent = "Topics\nI From y=mx+b to Newton's Math\n  A Motivating examples: Calculus-the study of change\n  B Speedometer in 29A\n  C Making the most of your resources\n  D Tangent lines revisit\nII From honest data to fake news\n  A Motivating examples: abuse statistics and evidence\n  B Fake news with statistical graphs and charts\n  C More fake news by numbers and statistics\n  D Misleading interest rate for marketing\nIII From a few data to educated guesses\n  A Motivating examples: data analytics\n  B What's the problem?\n  C Why is one prediction better than the others?\n  D Linear thinking in a nonlinear world\nIV From random bet to the best odds\n  A Motivating examples: probability and randomness\n  B Numerical experiments on random events\n  C Probability and area under a curve\n  D \"Let's make a deal\", hosted by Monty Hall\nTextbook\n1. Lecture notes prepared by the instructor\nReferences\n1. L. Redlin, S. Watson, and J. Stewart, Precalculus: Mathematics for Calculus, Belmont,\nCA : Thomson Brooks/Cole, 2006.\n2. K. E. Hirst, Calculus of One Variable, Springer, 2006.\n3. P.S. Mann, Introductory Statistics, 8th Ed., Wiley, 2013.",
            CILOs = new List<CILOs>
        {
            new CILOs
            {
                code = "CILO1",
                Description = "Explain the basic concept/theory in math/stat."
            },
            new CILOs
            {
                code = "CILO2",
                Description = "Illustrate the basic applications of the select topics in math/stat."
            },
            new CILOs
            {
                code = "CILO3",
                Description = "Design their own spreadsheet to solve real life math/stat problems."
            },
            new CILOs
            {
                code = "CILO4",
                Description = "Construct a perspective regarding the understanding of math/stat."
            }
        },
            TLAs = new List<TLAs>
        {
            new TLAs
            {
                code = new[] { "Lecture" },
                Description = "The instructor will present motivating examples of each topic to motivate students' interests and to introduce the topics of the course's materials. Basic concepts will be introduced to consolidate students' background knowledge. Examples on applications will be given to illustrate finer details."
            },
            new TLAs
            {
                code = new[] { "In-class activities", "Tutorial" },
                Description = "During classes and tutorials, students have the opportunity to participate in activities of various forms, including discussions, in-class exercises, programming / software practices."
            },
            new TLAs
            {
                code = new[] { "After-class activities" },
                Description = "Students are required to work on different assigned tasks after class and are encouraged to have further discussion with the instructor."
            }
        },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(math1025Version);

        var math1025Assessments = new List<CourseAssessment>
    {
        new CourseAssessment
        {
            CourseVersion = math1025Version,
            Name = "Online homework",
            Weighting = 20,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = math1025Version,
            Name = "Written Assignments",
            Weighting = 20,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = math1025Version,
            Name = "Oral assessment",
            Weighting = 20,
            Category = AssessmentCategory.Examination
        },
        new CourseAssessment
        {
            CourseVersion = math1025Version,
            Name = "In-class programming",
            Weighting = 20,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = math1025Version,
            Name = "Tests",
            Weighting = 20,
            Category = AssessmentCategory.Examination
        }
    };

        await context.CourseAssessments.AddRangeAsync(math1025Assessments);

        await context.SaveChangesAsync();

        return math1025Version;
    }

    // used to show anti-req of 4027, //TODO seed it to student's record 
    public static async Task<CourseVersion> math3836Gen(AppDbContext context, Code mathCode, Term semester1)
    {
        var math3836 = new Course
        {
            Name = "Data Mining",
            CourseNumber = "3836",
            CodeId = mathCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(math3836);

        var math3836Version = new CourseVersion
        {
            Course = math3836,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "This course introduces the concept of data mining and data mining techniques (including\nadvanced statistical and machine learning techniques)  for solving problems such as data\ncleaning, clustering, classification, relation detection, forecasting. It also introduces\nstudents to modern data mining applications such as recommendation systems and\nmining natural languages. Through participation in practical projects, the student will gain\nthe capability to drive forward innovation systems locally in Hong Kong as well as\ninternationally.",
            CourseContent = "Topics\nI Introduction\n  A The Knowledge Discovery Based in Databases (KDD)\n  B Data and Data Visualization\n  C Data Warehouse and Cloud storage\n  D Data Cleaning and Preprocessing\n  E Data Mining Principles\nII Mining Association Rules In Large Databases\n  A Association Rule Mining\n  B Mining Multidimensional Association Rules From Relational Databases\nIII Dimension Reduction techniques\n  A Principal Components Analysis\n  B Gaussian Process Latent Variable Model\n  C t-distributed Stochastic Neighbour Embedding\nIV Supervised Learning\n  A Neural Networks\n  B Linear and Partial Regression\n  C Metric Learning\nV Unsupervised Learning\n  A K - Means Clustering\n  B Gaussian Mixture Model\n  C Latent Dirichlet Allocation\nVI Recommendation Systems and mining natural language\n  A Collaborative Filtering\n  B Non-negative Matrix factorization\n  C Other advanced recommender algorithms\n  D Word and Sentence embedding\nTextbook\n1. Lecture notes prepared by the instructor\nReferences\n1. Pang-Ning Tan, Michael Steinbach, Anuj Karpatne, and Vipin Kumar, Introduction to\nData Mining (2nd Edition), Pearson, 2019\n2. Galit Shmueli, Peter C. Bruce, Inbal Yahav, Nitin R. Patel and Kenneth C. Lichtendahl\nJr., Data Mining for Business Analytics, Concepts, Techniques, and Applications, Wiley,\n2017\n3. Goodfellow, I.; Bengio, Y. & Courville, A., Deep Learning, MIT Press, 2016\n4. Jiawei Han, Micheline. Kamber and Jian Pei, Data Mining: Concepts and Techniques,\nThird Edition, The Morgan Kaufmann Publishers, 2011.\n5. Christopher M. Bishop, Pattern Recognition and Machine Learning, Springer, 2006.",
            CILOs = new List<CILOs>
        {
            new CILOs
            {
                code = "CILO1",
                Description = "Explain the fundamental principles of data mining"
            },
            new CILOs
            {
                code = "CILO2",
                Description = "Identify a working knowledge of data mining"
            },
            new CILOs
            {
                code = "CILO3",
                Description = "Interpret information from data mining"
            },
            new CILOs
            {
                code = "CILO4",
                Description = "Apply data mining skills and techniques"
            },
            new CILOs
            {
                code = "CILO5",
                Description = "Report the interpretation of findings in a scientific and concise manner"
            },
            new CILOs
            {
                code = "CILO6",
                Description = "Solve problems logically, analytically, critically and creatively"
            }
        },
            TLAs = new List<TLAs>
        {
            new TLAs
            {
                code = new[] { "Lecture" },
                Description = "Lectures with rigorous mathematical discussions and concrete examples. The lecturer will constantly ask questions in class to make sure that the majority of students are following the teaching materials. The lecture will also include Python programming examples to illustrate some of the concepts."
            },
            new TLAs
            {
                code = new[] { "In-class activity" },
                Description = "A problem-based approach will be used, using examples from real-life data mining problems in lectures to stimulate the learning of concepts, followed by software demos to consolidate the knowledge gained."
            },
            new TLAs
            {
                code = new[] { "Student Orientated Case Study" },
                Description = "A real-life case study of data mining application will be conducted using knowledge gained both during class, as well as from other findings of student(s)'s own research."
            }
        },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(math3836Version);

        var math3836Assessments = new List<CourseAssessment>
    {
        new CourseAssessment
        {
            CourseVersion = math3836Version,
            Name = "Tests",
            Weighting = 40,
            Category = AssessmentCategory.Examination
        },
        new CourseAssessment
        {
            CourseVersion = math3836Version,
            Name = "Project",
            Weighting = 35,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = math3836Version,
            Name = "Homework",
            Weighting = 25,
            Category = AssessmentCategory.Assignment
        }
    };

        await context.CourseAssessments.AddRangeAsync(math3836Assessments);

        await context.SaveChangesAsync();

        return math3836Version;
    }

    public static async Task<CourseVersion> comp4027Gen(AppDbContext context, Code compCode, Term semester1)
    {
        var comp4027 = new Course
        {
            Name = "Data Mining and Knowledge Discovery",
            CourseNumber = "4027",
            CodeId = compCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(comp4027);

        var comp4027Version = new CourseVersion
        {
            Course = comp4027,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "To learn the latest development of knowledge discovery and data mining concepts and\ntechniques. Theories and algorithms for data mining and knowledge discovery will be\nintroduced. Relevant applications in specific domains such as medicine and heath care\nwill be covered.",
            CourseContent = "I. Introduction to Data Mining\nA.  Overview of data mining\nB.  Data preparation for knowledge discovery\nC.  Data warehousing\nD.  Data visualization and exploration\nII. Data Mining Algorithms\nA.  Association rules\nB.  Apriori algorithms\nC.  Classification algorithms\nD.  Mining event sequences\nE.  Applications\nIII. Clustering\nA.  Classical algorithms\nB.  Graph-based clustering\nC.  Advanced methods\nD.  Applications\nIV. Case Studies in Data Mining\nA.  Health Informatics\nB.  Related applications",
            CILOs = new List<CILOs>
        {
            new CILOs
            {
                code = "CILO1",
                Description = "Distinguish data mining applications from other IT applications"
            },
            new CILOs
            {
                code = "CILO2",
                Description = "Explain data mining algorithms"
            },
            new CILOs
            {
                code = "CILO3",
                Description = "Explain applicability of data mining"
            },
            new CILOs
            {
                code = "CILO4",
                Description = "Suggest appropriate solutions to data mining problems"
            },
            new CILOs
            {
                code = "CILO5",
                Description = "Analyze data mining algorithms and techniques"
            },
            new CILOs
            {
                code = "CILO6",
                Description = "Work as a team in solving challenging data mining problems"
            }
        },
            TLAs = new List<TLAs>
        {
            new TLAs
            {
                code = new[] { "Lecture" },
                Description = "Student will learn the concepts from lecture"
            },
            new TLAs
            {
                code = new[] { "Assignment" },
                Description = "Student will achieve the outcomes via assignment on data mining"
            },
            new TLAs
            {
                code = new[] { "Laboratory" },
                Description = "Student will achieve the outcomes via guided laboratory with data mining software"
            },
            new TLAs
            {
                code = new[] { "Group Project" },
                Description = "Student will achieve the outcomes via group project on solving real world data mining problem"
            }
        },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp4027Version);

        var comp4027Assessments = new List<CourseAssessment>
    {
        new CourseAssessment
        {
            CourseVersion = comp4027Version,
            Name = "Continuous Assessment",
            Weighting = 40,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = comp4027Version,
            Name = "Examination",
            Weighting = 60,
            Category = AssessmentCategory.Examination
        }
    };

        await context.CourseAssessments.AddRangeAsync(comp4027Assessments);

        await context.SaveChangesAsync();

        return comp4027Version;
    }

    public static async Task<CourseVersion> comp4146Gen(AppDbContext context, Code compCode, Term semester1)
    {
        var comp4146 = new Course
        {
            Name = "Prompt Engineering for Generative AI",
            CourseNumber = "4146",
            CodeId = compCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(comp4146);

        var comp4146Version = new CourseVersion
        {
            Course = comp4146,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "This course introduces students to prompt engineering for generative AI systems. With\nthe advancement of large language models (LLMs)  and other generative AI technologies,\nthe ability to design effective prompts has become a critical skill. The course aims to\nprovide students with both theoretical understanding and practical skills in crafting\nprompts that effectively communicate with and control generative AI systems. Students\nwill learn the principles that govern different types of generative AI models, develop\ntechniques to optimize prompt design, and build applications that leverage these powerful\nsystems. Throughout the course, some emphasis will be placed on ethical considerations,\nevaluation methodologies, and the practical applications of prompt engineering in real\nworld scenarios.",
            CourseContent = "I. Introduction to Prompt Engineering and Generative AI Fundamentals\nA.    Introduction to Prompt Engineering\nB.    History and Evolution of Generative AI\nC.    Understanding Generative AI Models\nD.    Moving from Model Completion to Conversation\nII. Core Prompt Engineering Techniques\n   - Designing Generative AI Applications\n   - Prompt Content: Static and Dynamic\n   - Assembling Effective Prompts\n   - Optimizing Model Responses\nIII. Advanced Prompt Engineering Topics\nA.    Building Conversational Agents\nB.    Building Multi-Step Applications\nC.    Prompt-Based Fine-Tuning\nIV. Evaluation, Ethics, and Future Directions\nA.    Assessing Prompt and Model Quality\nB.    Ethical Considerations in Prompt Engineering\nC.    Multimodal Models",
            CILOs = new List<CILOs>
        {
            new CILOs
            {
                code = "CILO1",
                Description = "Describe the architecture and functions of generative AI models and explain the fundamental principles of prompt engineering."
            },
            new CILOs
            {
                code = "CILO2",
                Description = "Explain various prompt engineering techniques and their applications in different contexts."
            },
            new CILOs
            {
                code = "CILO3",
                Description = "Design and optimize prompts for various generative AI applications, including content generation, information extraction, and decision support systems."
            },
            new CILOs
            {
                code = "CILO4",
                Description = "Implement prompt engineering techniques like few-shot prompting, chain-of-thought reasoning, and retrieval-augmented generation."
            },
            new CILOs
            {
                code = "CILO5",
                Description = "Evaluate the quality of generative AI responses and implement techniques to mitigate hallucinations and biases in outputs."
            },
            new CILOs
            {
                code = "CILO6",
                Description = "Analyze the ethical considerations and limitations of prompt engineering in real-world applications."
            }
        },
            TLAs = new List<TLAs>
        {
            new TLAs
            {
                code = new[] { "Lecture" },
                Description = "Students will learn the fundamental principles and key concepts of prompt engineering via lectures."
            },
            new TLAs
            {
                code = new[] { "Tutorial", "In-class exercise", "Quiz" },
                Description = "Students will design and implement effective prompt engineering techniques, evaluate response quality, and assess ethical considerations through tutorials, in-class exercises, and quizzes."
            },
            new TLAs
            {
                code = new[] { "Assignment" },
                Description = "Students will work on written assignments to consolidate and apply what they have learned about prompt engineering and generative AI applications."
            }
        },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp4146Version);

        var comp4146Assessments = new List<CourseAssessment>
    {
        new CourseAssessment
        {
            CourseVersion = comp4146Version,
            Name = "Tutorial and quizzes",
            Weighting = 20,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = comp4146Version,
            Name = "Assignments",
            Weighting = 30,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = comp4146Version,
            Name = "Examination",
            Weighting = 50,
            Category = AssessmentCategory.Examination
        }
    };

        await context.CourseAssessments.AddRangeAsync(comp4146Assessments);

        await context.SaveChangesAsync();

        return comp4146Version;
    }

    public static async Task<CourseVersion> comp4046Gen(AppDbContext context, Code compCode, Term semester1)
    {
        var comp4046 = new Course
        {
            Name = "Information Systems Control and Auditing",
            CourseNumber = "4046",
            CodeId = compCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(comp4046);

        var comp4046Version = new CourseVersion
        {
            Course = comp4046,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "This course aims to give students a thorough grounding in the theory, techniques and\npractical issues involved in computer-based information systems control and\nauditing. The contents of this course include (but not limited to)  concepts, approaches,\nand techniques of information system auditing, security controls in organizations, and the\napplication of IT in auditing. This course reviews some basic concepts of computer\nsecurity.",
            CourseContent = "I. Introduction to IS Auditing\nA.  Objectives of IS audit and control\nB.  The structure of an IS audit and audit reports\nC.  IS auditing standards\nD.  Computer assisted audit tools\nII. Organization Security Controls\nA. Physical controls\nB. Logical controls\nC. Operational controls\nD. Personnel security and management practices\nE. Application software control\nIII. Systems and Network Security\nA. Systems and network security issues\nB. Non-malicious errors, virus and worms\nC. Access control, trusted operating systems\nD. Encryption in network\nE. User authentication\nF. E-mail security\nG. IP security\nH. Web security\nIV. Incident Handling and Computer Forensic",
            CILOs = new List<CILOs>
        {
            new CILOs
            {
                code = "CILO1",
                Description = "Illustrate the fundamental concepts of information systems auditing and IT application in auditing"
            },
            new CILOs
            {
                code = "CILO2",
                Description = "Identify the security controls in organization"
            },
            new CILOs
            {
                code = "CILO3",
                Description = "Explain the basic concepts of computer security, computer security threats and the corresponding remedies"
            },
            new CILOs
            {
                code = "CILO4",
                Description = "Describe the trend of computer security threats"
            },
            new CILOs
            {
                code = "CILO5",
                Description = "Apply physical, logical and operational security controls"
            },
            new CILOs
            {
                code = "CILO6",
                Description = "Assess the security of computer systems in terms of how well they are protected from computer security threats and integrate computer security mechanisms to protect computer systems from security threats"
            }
        },
            TLAs = new List<TLAs>
        {
            new TLAs
            {
                code = new[] { "Lecture" },
                Description = "Students will attend lectures for the concepts of IT auditing, security threats and controls."
            },
            new TLAs
            {
                code = new[] { "Case study", "Example" },
                Description = "Students will be provided with examples and cases to illustrate the topics covered."
            },
            new TLAs
            {
                code = new[] { "Exercise", "Assignment" },
                Description = "Students will work on exercises and assignments to consolidate their understanding on the covered topics."
            }
        },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp4046Version);

        var comp4046Assessments = new List<CourseAssessment>
    {
        new CourseAssessment
        {
            CourseVersion = comp4046Version,
            Name = "Continuous Assessment",
            Weighting = 40,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = comp4046Version,
            Name = "Examination",
            Weighting = 60,
            Category = AssessmentCategory.Examination
        }
    };

        await context.CourseAssessments.AddRangeAsync(comp4046Assessments);

        await context.SaveChangesAsync();

        return comp4046Version;
    }

    public static async Task<CourseVersion> comp3057Gen(AppDbContext context, Code compCode, Term semester1)
    {
        var comp3057 = new Course
        {
            Name = "Introduction to Artificial Intelligence and Machine Learning",
            CourseNumber = "3057",
            CodeId = compCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(comp3057);

        var comp3057Version = new CourseVersion
        {
            Course = comp3057,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "To introduce the fundamentals, models and techniques commonly found in artificial\nintelligence applications, and in particular, machine learning. To gain some hands-on\nexperience on developing machine learning solutions.",
            CourseContent = "I. Applied Mathematics\n   - Linear Algebra\n   - Probability and Information Theory\n   - Convex Optimization\n   - Numerical Computation\nII. Knowledge Representation\n   - Problem Solving (A* Search and Constraint Satisfaction)\n   - Knowledge, Reasoning and Planning (First-order Logic and Automated Planning)\n   - Uncertain Knowledge and Reasoning (Probabilistic Reasoning and Programming)\nIII. Learning Tasks\n   - Supervised Learning (Classification, Regression and Ranking)\n   - Unsupervised Learning (Clustering and Dimensionality Reduction)\n   - Reinforcement Learning (Q-learning and TD learning)\nIV. Basic Models\n   - Model Selection and Validation\n   - Decision Trees and Nearest Neighbor\n   - Support Vector Machines and Kernel Methods\n   - Generative and Discriminative Models\nV. Deep Models\n   - Deep Forward, Convolutional and Recurrent Networks\n   - Regularization and Optimization for Deep Learning",
            CILOs = new List<CILOs>
            {
                new CILOs
                {
                    code = "CILO1",
                    Description = "Describe the fundamentals of artificial intelligence and machine learning"
                },
                new CILOs
                {
                    code = "CILO2",
                    Description = "Explain classical AI and machine learning algorithms and their applications"
                },
                new CILOs
                {
                    code = "CILO3",
                    Description = "Describe machine learning models and algorithms"
                },
                new CILOs
                {
                    code = "CILO4",
                    Description = "Explain the capabilities, strengths and limitations of various AI and machine learning techniques"
                },
                new CILOs
                {
                    code = "CILO5",
                    Description = "Apply selected machine learning models and algorithms to solve real-world problems"
                },
                new CILOs
                {
                    code = "CILO6",
                    Description = "Evaluate available learning methods"
                }
            },
            TLAs = new List<TLAs>
            {
                new TLAs
                {
                    code = new[] { "Lecture", "Tutorial", "Laboratory" },
                    Description = "Students will learn the fundamentals, models and techniques in lectures. Examples of how to solve problems will be demonstrated in tutorials or labs to help students to have an understanding of the teaching materials."
                },
                new TLAs
                {
                    code = new[] { "Assignment", "Laboratory", "Mini Project" },
                    Description = "Students will work on assignments and labs to enhance the understanding of learning principles, and acquire hands-on experience from doing a mini project."
                }
            },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp3057Version);

        var comp3057Assessments = new List<CourseAssessment>
        {
            new CourseAssessment
            {
                CourseVersion = comp3057Version,
                Name = "Assignments and Lab Exercises",
                Weighting = 30,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = comp3057Version,
                Name = "Mini Project",
                Weighting = 20,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = comp3057Version,
                Name = "Examination",
                Weighting = 50,
                Category = AssessmentCategory.Examination
            }
        };

        await context.CourseAssessments.AddRangeAsync(comp3057Assessments);

        await context.SaveChangesAsync();

        return comp3057Version;
    }

    public static async Task<CourseVersion> comp2876Gen(AppDbContext context, Code compCode, Term semester1)
    {
        var comp2876 = new Course
        {
            Name = "Demo Prerequisite Course",
            CourseNumber = "2876",
            CodeId = compCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(comp2876);

        var comp2876Version = new CourseVersion
        {
            Course = comp2876,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "",
            CourseContent = "",
            CILOs = new List<CILOs>
        {
            new CILOs { code = "CILO1", Description = "Demonstrate foundational knowledge required for prerequisite validation" },
            new CILOs { code = "CILO2", Description = "Apply basic concepts in a structured problem-solving context" },
            new CILOs { code = "CILO3", Description = "Identify and explain key terminology relevant to the prerequisite domain" }
        },
            TLAs = new List<TLAs>
        {
            new TLAs { code = new[] { "Lecture" }, Description = "Students will attend lectures covering core prerequisite concepts." },
            new TLAs { code = new[] { "Tutorial" }, Description = "Students will complete guided exercises to reinforce prerequisite knowledge." }
        },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp2876Version);

        var comp2876Assessments = new List<CourseAssessment>
    {
        new CourseAssessment
        {
            CourseVersion = comp2876Version,
            Name = "Assignments",
            Weighting = 40,
            Category = AssessmentCategory.Assignment
        },
        new CourseAssessment
        {
            CourseVersion = comp2876Version,
            Name = "Examination",
            Weighting = 60,
            Category = AssessmentCategory.Examination
        }
    };

        await context.CourseAssessments.AddRangeAsync(comp2876Assessments);

        await context.SaveChangesAsync();

        return comp2876Version;
    }

    public static async Task<CourseVersion> comp2877Gen(AppDbContext context, Code compCode, Term semester1)
    {
        var comp2877 = new Course
        {
            Name = "Demo Intermediate Computing Course",
            CourseNumber = "2877",
            CodeId = compCode.Id,
            Credit = 3,
            Description = ""
        };

        await context.Courses.AddAsync(comp2877);

        var comp2877Version = new CourseVersion
        {
            Course = comp2877,
            VersionNumber = 1,
            Description = "",
            AimAndObjectives = "",
            CourseContent = "",
            CILOs = new List<CILOs>
            {
                new CILOs { code = "CILO1", Description = "Explain intermediate computing concepts used in subsequent courses" },
                new CILOs { code = "CILO2", Description = "Apply problem-solving techniques to moderately complex computing tasks" },
                new CILOs { code = "CILO3", Description = "Analyze basic system designs and implementation choices" },
                new CILOs { code = "CILO4", Description = "Demonstrate readiness for advanced-level computing courses" }
            },
            TLAs = new List<TLAs>
            {
                new TLAs
                {
                    code = new[] { "Lecture" },
                    Description = "Students will learn intermediate computing concepts through structured lectures."
                },
                new TLAs
                {
                    code = new[] { "Tutorial", "Laboratory" },
                    Description = "Students will practice and apply concepts through guided tutorials and laboratory exercises."
                }
            },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp2877Version);

        var comp2877Assessments = new List<CourseAssessment>
        {
            new CourseAssessment
            {
                CourseVersion = comp2877Version,
                Name = "Assignments and Lab Exercises",
                Weighting = 40,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = comp2877Version,
                Name = "Examination",
                Weighting = 60,
                Category = AssessmentCategory.Examination
            }
        };

        await context.CourseAssessments.AddRangeAsync(comp2877Assessments);

        await context.SaveChangesAsync();

        return comp2877Version;
    }



    public static async Task seedCourses(AppDbContext context, UserManager<User> userManager)
    {
        // the term is just for create record purpose
        var semester1 = await context.Terms.FirstAsync(t => t.Id == 1);
        var semester2 = await context.Terms.FirstAsync(t => t.Id == 2);

        var compCode = await context.Codes.FirstAsync(c => c.Tag == "COMP");
        var mathCode = await context.Codes.FirstAsync(c => c.Tag == "MATH");

        var compSciCore001 = await context.CourseGroups.FirstAsync(c => c.Name == "Core Courses - COMPSCI-CORE-001");
        var freeElective001 = await context.CourseGroups.FirstAsync(c => c.Name == "Free Elective Courses");
        var compISAElective001 =
            await context.CourseGroups.FirstAsync(c => c.Name == "ISA Elective Courses - COMP-ISA-ELEC-001");
        var compISAElective002 =
            await context.CourseGroups.FirstAsync(c => c.Name == "ISA Elective Courses - COMP-ISA-ELEC-002");
        var compISAElective003 =
            await context.CourseGroups.FirstAsync(c => c.Name == "ISA Elective Courses - COMP-ISA-ELEC-003");


        var comp1005Version = await comp1005Gen(context, compCode, semester1);
        var math1025Version = await math1025Gen(context, mathCode, semester1);
        var math3836Version = await math3836Gen(context, mathCode, semester1);
        var comp4027Version = await comp4027Gen(context, compCode, semester1);
        var comp4046Version = await comp4046Gen(context, compCode, semester1);
        var comp4146Version = await comp4146Gen(context, compCode, semester1);
        var comp3057Version = await comp3057Gen(context, compCode, semester1);

        //This is a fake coruse for demo
        var comp2876Version = await comp2876Gen(context, compCode, semester1);
        var comp2877Version = await comp2877Gen(context, compCode, semester1);

        await LinkCourseIntoCourseGroups(context, comp1005Version.Id, compSciCore001.Id);
        await LinkCourseIntoCourseGroups(context, math1025Version.Id, compSciCore001.Id);

        await LinkCourseIntoCourseGroups(context, comp4027Version.Id, compISAElective001.Id);

        await LinkCourseIntoCourseGroups(context, comp4046Version.Id, compISAElective002.Id);

        await LinkCourseIntoCourseGroups(context, comp4146Version.Id, compISAElective003.Id);
        await LinkCourseIntoCourseGroups(context, comp3057Version.Id, compISAElective003.Id);

        math3836Version.AntiRequisites.Add(
            new CourseAntiReq()
            {
                ExcludedCourseVersion = comp4027Version
            }
        );

        comp4027Version.AntiRequisites.Add(
            new CourseAntiReq()
            {
                ExcludedCourseVersion = math3836Version
            }
        );

        comp2877Version.Prerequisites.Add(
            new CoursePreReq()
            {
                RequiredCourseVersion = comp2876Version
            });




        // Simple record that used to show anti req
        string studentEmail = "student@bu.edu";
        User? studentUser = await userManager.FindByEmailAsync(studentEmail);

        if (studentUser != null)
        {
            studentUser.StudentCourses.Add(
                new StudentCourse()
                {
                    Course = math3836Version.Course,
                    AcademicYear = 2025,
                    Term = semester1,
                    Status = StudentCourseStatus.Completed,
                    Grade = Grade.A,
                    Notes = ""
                }
            );
        }

        // also create section here so I don't need to get those ID again
        // sections 2025 sem 2

        var comp4027Section1 = new CourseSection()
        {
            CourseVersion = comp4027Version,
            Year = 2025,
            Term = semester2,
            SectionNumber = 1
        };

        var comp4027Section2 = new CourseSection()
        {
            CourseVersion = comp4027Version,
            Year = 2025,
            Term = semester2,
            SectionNumber = 2
        };

        await context.CourseSections.AddRangeAsync(new[]
        {
            comp4027Section1,
            comp4027Section2
        });

        comp4027Section1.CourseMeetings.Add(
            new CourseMeeting()
            {
                Day = 1,
                MeetingType = "Lecture",
                StartTime = new TimeOnly(11, 30),
                EndTime = new TimeOnly(14, 20)
            }
        );
        comp4027Section1.CourseMeetings.Add(
            new CourseMeeting()
            {
                Day = 3,
                MeetingType = "Lab",
                StartTime = new TimeOnly(11, 30),
                EndTime = new TimeOnly(14, 20)
            }
        );

        comp4027Section2.CourseMeetings.Add(
            new CourseMeeting()
            {
                Day = 2, // Tue
                MeetingType = "Lecture",
                StartTime = new TimeOnly(9, 30),
                EndTime = new TimeOnly(10, 20)
            }
        );

        comp4027Section2.CourseMeetings.Add(
            new CourseMeeting()
            {
                Day = 4, // Thu
                MeetingType = "Lab",
                StartTime = new TimeOnly(9, 30),
                EndTime = new TimeOnly(10, 20)
            }
        );

        var comp4046Section1 = new CourseSection()
        {
            CourseVersion = comp4046Version,
            Year = 2025,
            Term = semester2,
            SectionNumber = 1
        };

        var comp4046Section2 = new CourseSection()
        {
            CourseVersion = comp4046Version,
            Year = 2025,
            Term = semester2,
            SectionNumber = 2
        };

        await context.CourseSections.AddRangeAsync(new[]
        {
            comp4046Section1,
            comp4046Section2
        });

        // Section A meetings
        comp4046Section1.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 1, // Mon
            MeetingType = "Lecture",
            StartTime = new TimeOnly(15, 30),
            EndTime = new TimeOnly(16, 20)
        });
        comp4046Section1.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 3, // Wed
            MeetingType = "Lecture",
            StartTime = new TimeOnly(15, 30),
            EndTime = new TimeOnly(16, 20)
        });

        comp4046Section2.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 2, // Tue
            MeetingType = "Lecture",
            StartTime = new TimeOnly(11, 30),
            EndTime = new TimeOnly(12, 20)
        });
        comp4046Section2.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 4, // Thu
            MeetingType = "Lab",
            StartTime = new TimeOnly(11, 30),
            EndTime = new TimeOnly(12, 20)
        });

        var comp4146Section1 = new CourseSection()
        {
            CourseVersion = comp4146Version,
            Year = 2025,
            Term = semester2,
            SectionNumber = 1
        };

        var comp4146Section2 = new CourseSection()
        {
            CourseVersion = comp4146Version,
            Year = 2025,
            Term = semester2,
            SectionNumber = 2
        };

        await context.CourseSections.AddRangeAsync(new[]
        {
            comp4146Section1,
            comp4146Section2
        });

        // Section A meetings
        comp4146Section1.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 2, // Tue
            MeetingType = "Lecture",
            StartTime = new TimeOnly(9, 30),
            EndTime = new TimeOnly(10, 20)
        });
        comp4146Section1.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 4, // Thu
            MeetingType = "Lab",
            StartTime = new TimeOnly(9, 30),
            EndTime = new TimeOnly(10, 20)
        });

        // Section B meetings
        comp4146Section2.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 1, // Mon
            MeetingType = "Lecture",
            StartTime = new TimeOnly(10, 30),
            EndTime = new TimeOnly(11, 20)
        });
        comp4146Section2.CourseMeetings.Add(new CourseMeeting()
        {
            Day = 4, // Wed
            MeetingType = "Lab",
            StartTime = new TimeOnly(11, 30),
            EndTime = new TimeOnly(12, 20)
        });


        await context.SaveChangesAsync();
    }







    public static async Task LinkCourseIntoCourseGroups(AppDbContext context, int courseVersionId, int courseGroupId)
    {
        var courseId = await context.CourseVersions
            .Where(cv => cv.Id == courseVersionId)
            .Select(cv => (int?)cv.CourseId)
            .SingleOrDefaultAsync();

        if (courseId is null)
            throw new InvalidOperationException($"CourseVersion with id={courseVersionId} was not found.");

        var groupExists = await context.CourseGroups.AnyAsync(g => g.Id == courseGroupId);
        if (!groupExists)
            throw new InvalidOperationException($"CourseGroup with id={courseGroupId} was not found.");
        await context.GroupCourses.AddAsync(new GroupCourse
        {
            GroupId = courseGroupId,
            CourseId = courseId.Value,
            CodeId = null
        });

        await context.SaveChangesAsync();
    }

    private static async Task GenerateCourseEmbeddings(CourseVersion courseVersion, List<CourseAssessment> assessments,
        IAIProvider aiProvider, Course course)
    {
        try
        {
            // Convert CourseAssessment to AssessmentMethod for embedding generation
            var assessmentMethods = assessments.Select(a => new AssessmentMethod
            {
                Name = a.Name,
                Weighting = a.Weighting,
                Category = a.Category,
                Description = ""
            }).ToList();

            // Generate domain tag embedding
            var domainEmbedding = await aiProvider.CreateCourseDomainTagEmbeddingAsync(
                courseTitle: course.Name,
                aimsAndObjectives: courseVersion.AimAndObjectives,
                courseContent: courseVersion.CourseContent
            );
            Console.WriteLine($"Generated domain embedding for course: {course.Name}");

            // Generate skills tag embedding
            var skillsEmbedding = await aiProvider.CreateCourseSkillsTagEmbeddingAsync(
                aimsAndObjectives: courseVersion.AimAndObjectives,
                cilos: courseVersion.CILOs,
                courseContent: courseVersion.CourseContent,
                tlas: courseVersion.TLAs,
                assessmentMethods: assessmentMethods
            );
            Console.WriteLine($"Generated skills embedding for course: {course.Name}");

            // Generate content types tag embedding
            var contentTypesEmbedding = await aiProvider.CreateCourseContentTypesTagEmbeddingAsync(
                courseContent: courseVersion.CourseContent,
                tlas: courseVersion.TLAs,
                assessmentMethods: assessmentMethods
            );
            Console.WriteLine($"Generated content types embedding for course: {course.Name}");

            // Store embeddings (for now, just logging - you can add storage logic later)
            Console.WriteLine($"Successfully generated all embeddings for: {course.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to generate embeddings for course {course.Name}: {ex.Message}");
        }
    }

}