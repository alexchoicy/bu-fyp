using Backend.Models;
using Backend.Services.AI;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public class TagSeed
{
    public static async Task SeedAsync(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        await SeedSkillTagsAsync(context, aiProviderFactory);
        await SeedDomainTagsAsync(context, aiProviderFactory);
        await SeedContentTypeTagsAsync(context, aiProviderFactory);
    }

    private static async Task SeedSkillTagsAsync(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        if (await context.Tags.AnyAsync(t => t.TagType == TagType.Skill))
            return;

        var skills = new Dictionary<string, string>
        {
            { "Programming", "Ability to write, read, and debug code in one or more programming languages to solve computational problems or build software." },
            { "Software Development", "End-to-end process of designing, implementing, testing, and maintaining software systems or applications." },
            { "Data Analysis", "Collecting, cleaning, and examining data to uncover patterns, trends, and insights for decision-making." },
            { "Statistics", "Using statistical methods, probability, and inference to analyze data and draw conclusions under uncertainty." },
            { "Mathematics", "Applying mathematical concepts and techniques such as algebra, calculus, and discrete math to solve quantitative problems." },
            { "Logic & Reasoning", "Using structured, rational thinking and formal logic to evaluate arguments, detect fallacies, and reach sound conclusions." },
            { "Problem Solving", "Identifying problems, generating possible solutions, and selecting effective strategies to resolve them." },
            { "Critical Thinking", "Evaluating information and arguments carefully, questioning assumptions, and judging reliability and relevance." },
            { "Algorithmic Thinking", "Breaking down problems into clear, step-by-step procedures that can be implemented or automated." },
            { "Engineering Skills", "Applying engineering principles, tools, and methods to design, build, and evaluate systems or products." },
            { "Quantitative Modeling", "Building mathematical or computational models to represent real-world systems and support analysis or prediction." },
            { "Scientific Analysis", "Using scientific methods to interpret data, test hypotheses, and validate or refute explanations." },
            { "Research Methods", "Designing studies, collecting data, and using appropriate methodologies to investigate research questions." },
            { "Academic Writing", "Writing structured, formal documents with citations, arguments, and evidence suitable for scholarly contexts." },
            { "Literature Review", "Surveying, organizing, and synthesizing existing research to understand the state of knowledge on a topic." },
            { "Experimentation", "Designing and running experiments, controlling variables, and analyzing results to test hypotheses." },
            { "Data Interpretation", "Making sense of data outputs, charts, and statistics to draw meaningful, context-aware conclusions." },
            { "Theory Building", "Developing abstract frameworks or models that explain observed phenomena and can be tested or refined." },
            { "Writing", "Producing clear, coherent text for various purposes, audiences, and formats, from short pieces to long-form documents." },
            { "Public Speaking", "Delivering spoken presentations or talks clearly and confidently to groups or audiences." },
            { "Communication", "Exchanging information effectively through spoken, written, or visual means, tailored to the audience." },
            { "Collaboration", "Working productively with others, sharing responsibilities, and coordinating efforts toward shared goals." },
            { "Leadership", "Guiding individuals or teams, making decisions, and taking responsibility for direction and outcomes." },
            { "Creativity", "Generating original, useful ideas or approaches and combining existing concepts in novel ways." },
            { "Design Thinking", "Using a user-centered, iterative process to understand problems, prototype solutions, and refine designs." },
            { "Negotiation", "Reaching mutually acceptable agreements through discussion, compromise, and strategic communication." },
            { "Presentation Skills", "Designing and delivering engaging, structured presentations using visual aids and clear explanations." },
            { "Lab Skills", "Performing practical tasks in a lab environment, including using instruments, following protocols, and ensuring safety." },
            { "Field Work", "Collecting data or observations in real-world settings outside the lab or classroom." },
            { "Project Management", "Planning, organizing, and tracking tasks, resources, and timelines to complete projects successfully." },
            { "Technical Writing", "Creating clear, precise documentation for technical processes, systems, or products." },
            { "Hands-on Practice", "Learning and applying skills through direct, practical activities and physical manipulation of tools or materials." },
            { "Applied Engineering", "Implementing engineering concepts in real-world contexts to design and improve functional systems or products." },
            { "Clinical Skills", "Applying practical, patient-facing or case-based skills in healthcare or clinical environments." }
        };

        var skillTags = skills.Select(skill => new Tag
        {
            Name = skill.Key,
            TagType = TagType.Skill,
            Description = skill.Value
        }).ToList();

        // Generate embeddings if AI provider is available
        if (aiProviderFactory != null)
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            foreach (var tag in skillTags)
            {
                if (!string.IsNullOrEmpty(tag.Description))
                {
                    try
                    {
                        tag.Embedding = await aiProvider.CreateEmbeddingAsync(tag.Description);
                        Console.WriteLine($"Generated embedding for skill tag: {tag.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to generate embedding for skill tag {tag.Name}: {ex.Message}");
                    }
                }
            }
        }

        await context.Tags.AddRangeAsync(skillTags);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDomainTagsAsync(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        if (await context.Tags.AnyAsync(t => t.TagType == TagType.Domain))
            return;

        var domains = new Dictionary<string, string>
        {
            { "Computer Science", "Study of computation, algorithms, data structures, and the design of computer systems and software." },
            { "Information Technology", "Practical use and management of computers, networks, and systems to support organizational needs." },
            { "Engineering", "Application of scientific and mathematical principles to design, build, and maintain structures, devices, and systems." },
            { "Mathematics", "Formal study of numbers, structures, patterns, and change, including pure and applied branches." },
            { "Physics", "Study of matter, energy, motion, and fundamental forces governing the physical universe." },
            { "Chemistry", "Study of substances, their composition, structure, properties, and the reactions between them." },
            { "Biology", "Study of living organisms, their structure, function, evolution, and interactions." },
            { "Environmental Science", "Interdisciplinary study of the environment and human impact on ecosystems and natural systems." },
            { "Health Science", "Study of health, wellness, and factors that influence physical and mental well-being." },
            { "Medicine", "Science and practice of diagnosing, treating, and preventing illness and disease in humans." },
            { "Business", "Study of how organizations create value through strategy, operations, finance, and markets." },
            { "Management", "Planning, organizing, and coordinating people and resources to achieve organizational goals." },
            { "Finance", "Management and analysis of money, investments, risk, and financial markets." },
            { "Accounting", "Recording, analyzing, and reporting financial transactions and information for decision-making and compliance." },
            { "Marketing", "Understanding customer needs and creating, communicating, and delivering value through products and services." },
            { "Economics", "Study of how individuals, firms, and societies allocate scarce resources and respond to incentives." },
            { "Entrepreneurship", "Creation and growth of new ventures, including opportunity recognition, innovation, and risk-taking." },
            { "Psychology", "Study of human behavior, cognition, emotions, and mental processes." },
            { "Sociology", "Study of social groups, institutions, cultures, and patterns of human interaction." },
            { "History", "Study of past events, societies, and processes and how they shape the present." },
            { "Philosophy", "Study of fundamental questions about existence, knowledge, ethics, logic, and meaning." },
            { "Literature", "Study of written works, including their themes, styles, contexts, and interpretations." },
            { "Linguistics", "Scientific study of language structure, use, acquisition, and variation." },
            { "Political Science", "Study of political systems, power, governance, public policy, and international relations." },
            { "Education", "Study of teaching, learning processes, curricula, and educational systems." },
            { "Cultural Studies", "Interdisciplinary analysis of cultural practices, identities, media, and power relations." },
            { "Fine Arts", "Creation and study of visual and performing arts such as painting, sculpture, and performance." },
            { "Graphic Design", "Visual communication and problem-solving using typography, imagery, layout, and visual composition." },
            { "Music", "Study and practice of musical performance, theory, composition, and sound." },
            { "Media Studies", "Study of media content, industries, technologies, and their influence on society and culture." },
            { "Architecture", "Design of buildings and physical spaces, balancing aesthetics, function, and structural requirements." }
        };

        var domainTags = domains.Select(domain => new Tag
        {
            Name = domain.Key,
            TagType = TagType.Domain,
            Description = domain.Value
        }).ToList();

        // Generate embeddings if AI provider is available
        if (aiProviderFactory != null)
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            foreach (var tag in domainTags)
            {
                if (!string.IsNullOrEmpty(tag.Description))
                {
                    try
                    {
                        tag.Embedding = await aiProvider.CreateEmbeddingAsync(tag.Description);
                        Console.WriteLine($"Generated embedding for domain tag: {tag.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to generate embedding for domain tag {tag.Name}: {ex.Message}");
                    }
                }
            }
        }

        await context.Tags.AddRangeAsync(domainTags);
        await context.SaveChangesAsync();
    }

    private static async Task SeedContentTypeTagsAsync(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        if (await context.Tags.AnyAsync(t => t.TagType == TagType.ContentType))
            return;

        var contentTypes = new Dictionary<string, string>
        {
            { "Theoretical", "Focused on abstract concepts, principles, and frameworks rather than direct practical application." },
            { "Conceptual", "Emphasizes understanding key ideas, models, and relationships without heavy technical detail or calculation." },
            { "Reading-Heavy", "Requires substantial reading of texts, articles, or books as the main mode of learning." },
            { "Writing-Heavy", "Involves frequent written assignments, essays, or reports as primary outputs." },
            { "Discussion-Based", "Relies on class discussions, debates, and dialogue as core learning activities." },
            { "Lab-Based", "Centers on laboratory sessions with experiments, measurements, and hands-on technical work." },
            { "Workshop-Based", "Organized around interactive sessions focused on practicing specific skills or methods." },
            { "Studio-Based", "Involves creative work in a studio setting with iterative making, critique, and revision." },
            { "Exam-Heavy", "Assessment is primarily through quizzes, tests, or formal exams." },
            { "Project-Based", "Learning and assessment are structured around larger projects or deliverables over time." },
            { "Assignment-Heavy", "Frequent smaller tasks, problem sets, or homework form a major part of the workload." },
            { "Presentation-Based", "Relies heavily on individual or group presentations for sharing work and assessment." },
            { "Group Work", "Emphasizes collaborative tasks, team projects, and shared responsibility for outcomes." },
            { "Lecture-Based", "Primarily delivered through instructor-led lectures, often with supporting materials or slides." }
        };

        var contentTypeTags = contentTypes.Select(contentType => new Tag
        {
            Name = contentType.Key,
            TagType = TagType.ContentType,
            Description = contentType.Value
        }).ToList();

        // Generate embeddings if AI provider is available
        if (aiProviderFactory != null)
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            foreach (var tag in contentTypeTags)
            {
                if (!string.IsNullOrEmpty(tag.Description))
                {
                    try
                    {
                        tag.Embedding = await aiProvider.CreateEmbeddingAsync(tag.Description);
                        Console.WriteLine($"Generated embedding for content type tag: {tag.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to generate embedding for content type tag {tag.Name}: {ex.Message}");
                    }
                }
            }
        }

        await context.Tags.AddRangeAsync(contentTypeTags);
        await context.SaveChangesAsync();
    }
}
