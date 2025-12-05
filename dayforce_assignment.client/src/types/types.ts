export interface TestCase {
    testName: string;
    preconditions: string;
    steps: string;
    expectedResult: string;
}

export interface ConfluencePage {
    id: string;
    title: string;
}

export interface ApiResponse {
    status: string;
    jiraTitle: string;
    confluencePagesConsidered: ConfluencePage[];
    testCases: TestCase[];
}
